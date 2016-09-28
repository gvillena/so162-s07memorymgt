﻿Imports System.Text

Public Class ParticionFija

#Region " Fields "

    ' Administrador de Memoria en Ejecucion
    Private Ejecucion As Boolean = False

    ' Algoritmo de Asignación
    Private Algoritmo As AlgoritmoAsignacion

    ' Tamaño de Memoria
    Private TamañoMemoria As Integer = 0

    ' Lista de Procesos 
    Private ListaProcesos As List(Of Proceso)

    ' Lista de Particiones
    Private ListaParticiones As List(Of Particion)

    ' Lista de Particiones Libres
    Private ParticionesLibres As List(Of Particion)

    ' Lista de Particiones Ocupadas
    Private ParticionesOcupadas As List(Of Particion)

#End Region

#Region " Subs & Functions"

    Public Sub Inicializar(_tamañoMemoria As Integer, _listaParticiones As List(Of Particion), _algoritmo As AlgoritmoAsignacion)

        ' Tamaño de Memoria
        TamañoMemoria = _tamañoMemoria

        ' Inicializando Particiones 
        ListaParticiones = _listaParticiones

        ' Inicializando Particiones Libres
        ParticionesLibres = New List(Of Particion)
        For Each p As Particion In _listaParticiones
            ParticionesLibres.Add(p)
        Next

        ' Inicializando Particiones Ocupadas
        ParticionesOcupadas = New List(Of Particion)

        ' Actualizando Etiqueta de Particiones
        Dim sbParticiones As StringBuilder = New StringBuilder()
        For Each p As Particion In _listaParticiones
            sbParticiones.Append(String.Format("P{0}: {1}kb | ", p.Numero, p.Tamaño))
        Next
        lblParticiones.Text = sbParticiones.ToString(0, sbParticiones.Length - 2)

        ' Actualizando Etiqueta de Tamaño de Memoria
        lblMemoria.Text = String.Format("Memoria Ram ~ {0}kb", _tamañoMemoria)

        ' Estableciendo algoritmo de asignacion
        Algoritmo = _algoritmo

        ' Actualizando Etiqueta de Algoritmo de Asignacion
        Select Case Algoritmo
            Case AlgoritmoAsignacion.PrimerAjuste
                lblAlgoritmo.Text = "Algoritmo de Asignación -> Primer Ajuste"
            Case AlgoritmoAsignacion.MejorAjuste
                lblAlgoritmo.Text = "Algoritmo de Asignación -> Mejor Ajuste"
        End Select

    End Sub

    Private Sub AsignarEspacio()

        ' Verificar si se esta ejecutando el administrador de memoria 
        If Not Ejecucion Then Exit Sub

        ' Verificar si existen procesos en la lista de procesos
        If ListaProcesos.Count > 0 Then

            Dim index As Integer = 0
            Dim ptAux As Particion = Nothing
            Dim msj As String = String.Empty

            For Each proceso As Proceso In ListaProcesos

                ' Si el proceso no esta en espera pasar al siguiente
                If Not proceso.Estado = EstadoProceso.Espera Then
                    Continue For
                End If

                ' Buscando indice de particion libre (primer ajuste)
                index = BuscarParticionlibre(proceso.Tamaño)

                ' Si se encontro particion index (>=) a cero
                If index >= 0 Then
                    ' Obteniendo particion de la particiones libres
                    ptAux = ParticionesLibres.Item(index)
                    ' Cambiando el estado de la particion a Ocupada
                    ptAux.Estado = EstadoParticion.Ocupada
                    ' Cambiando el estado del proceso a Asignado 
                    ListaProcesos.Item(ListaProcesos.IndexOf(proceso)).Estado = EstadoProceso.Asignado
                    ' Estableciendo hora de inicio
                    ListaProcesos.Item(ListaProcesos.IndexOf(proceso)).HoraInicio = Date.Now()
                    ' Estableciendo direccion de memoria asiganada
                    ListaProcesos.Item(ListaProcesos.IndexOf(proceso)).DireccionMemoria = ptAux.Direccion

                    ' Asignando el proceso a la particion
                    ptAux.Proceso = proceso
                    ' Agregando particion a lista de particiones ocupadas
                    ParticionesOcupadas.Add(ptAux)
                    ' Eliminando particion de lista de particiones librs 
                    ParticionesLibres.RemoveAt(index)
                    ' Mostrando mensaje en registro (log)
                    msj = String.Format("El proceso {0} de tamaño {1}kb ha sido asignado a la partición de memoria {2} con dirección {3}", proceso.Nombre, proceso.Tamaño, ptAux.Numero, ptAux.Direccion)
                Else ' de lo contrario
                    msj = String.Format("El proceso {0} de tamaño {1}kb no ha podido ser asignado debido a memoria insuficiente.", proceso.Nombre, proceso.Tamaño)
                End If

                ' Mostrando fecha y hora en registro (log)
                txtSalida.AppendText(String.Format("[{0}]", Date.Now.ToString("dd/MM/yyyy hh:mm:ss tt")))
                txtSalida.AppendText(vbCrLf + msj + vbCrLf + vbCrLf)
            Next

            ' Actualizando controles
            ActualizarDgvParticiones()
            ActualizaDgvProcesos()
        Else
            ' Si no hay procesos en cola mostrar en registro
            txtSalida.AppendText(String.Format("[{0}]", Date.Now.ToString("dd/MM/yyyy hh: mm:ss tt")))
            txtSalida.AppendText(vbCrLf + "No hay procesos en cola." + vbCrLf + vbCrLf)
        End If

    End Sub

    Private Sub TerminarProceso(nomProcSel)

        ' Declarando variables
        Dim index As Integer = 0
        Dim ptAux As Particion = Nothing
        Dim procAux As Proceso = Nothing

        ' Buscando nombre proceso en particiones ocupadas 
        For Each p As Particion In ParticionesOcupadas
            If p.Proceso.Nombre = nomProcSel Then
                index = ParticionesOcupadas.IndexOf(p)
                ptAux = p
                procAux = p.Proceso
                Exit For
            End If
        Next

        ' Mostrando mensaje en registro (log)
        txtSalida.AppendText(String.Format("[{0}]", Date.Now.ToString("dd/MM/yyyy hh:mm:ss tt")))
        txtSalida.AppendText(vbCr + String.Format("El proceso {0} de tamaño {1}k ha sido terminado", ptAux.Proceso.Nombre, ptAux.Proceso.Tamaño))
        txtSalida.AppendText(vbCr + String.Format("La partición {0} con dirección {1} ha sido liberada.", ptAux.Numero, ptAux.Direccion))
        txtSalida.AppendText(vbCr + vbCr)

        ' Actualizando informacion del proceso
        ListaProcesos.Item(ListaProcesos.IndexOf(procAux)).Estado = EstadoProceso.Terminado

        ' Estableciendo Hora Fin
        ListaProcesos.Item(ListaProcesos.IndexOf(procAux)).HoraFin = Date.Now()

        ' Actualizando informacion de la particion
        ptAux.Proceso = Nothing
        ptAux.Estado = EstadoParticion.Libre

        ' Eliminando particion de lista de particiones ocupadas
        ParticionesOcupadas.RemoveAt(index)

        ' Agregando particion a lista de particiones libres
        ParticionesLibres.Add(ptAux)

        ' Actualizando controles
        ActualizarDgvParticiones()

        ' Asignar Espacio
        AsignarEspacio()

    End Sub

    Private Function BuscarParticionlibre(tamañoProceso As Integer) As Integer

        If Algoritmo = AlgoritmoAsignacion.PrimerAjuste Then
            Return BuscarParticionLibrePrimerAjuste(tamañoProceso)
        End If

        If Algoritmo = AlgoritmoAsignacion.MejorAjuste Then
            Return BuscarParticionLibreMejorAjuste(tamañoProceso)
        End If

        Return 0
    End Function

    Private Function BuscarParticionLibrePrimerAjuste(tamañoProceso As Integer) As Integer
        Dim index As Integer = -1
        For Each particion As Particion In ListaParticiones

            ' Verificando si el tamaño-de-la-particion no es (>=) al tamaño-del-proceso
            If Not particion.Tamaño >= tamañoProceso Then
                Continue For
            End If

            ' Verificando si la particion no se encuentra en la lista de particiones libres
            If ParticionesLibres.IndexOf(particion) = -1 Then
                Continue For
            End If

            ' Devolviendo el indice de la particion libre encontrada 
            index = ParticionesLibres.IndexOf(particion)
            Exit For
        Next
        Return index
    End Function

    Private Function BuscarParticionLibreMejorAjuste(tamañoProceso As Integer) As Integer
        Dim index As Integer = -1
        For Each particion As Particion In ListaParticiones

            ' Verificando si el tamaño-de-la-particion no es (>=) al tamaño-del-proceso
            If Not particion.Tamaño >= tamañoProceso Then
                Continue For
            End If

            ' Verificando si la particion no se encuentra en la lista de particiones libres
            If ParticionesLibres.IndexOf(particion) = -1 Then
                Continue For
            End If

            ' Si no se encontro previamente particion
            If index = -1 Then

                ' Indice de la particion libre encontrada 
                index = ParticionesLibres.IndexOf(particion)

                ' Saltar la comparación de particiones
                Continue For
            End If

            ' Comparando tamaño de particiones
            Dim TamañoP1 As Integer = ParticionesLibres.ElementAt(index).Tamaño
            Dim TamañoP2 As Integer = particion.Tamaño
            If TamañoP1 > TamañoP2 Then
                ' Indice de la particion libre encontrada que mejor se ajusta
                index = ParticionesLibres.IndexOf(particion)
            End If

        Next
        Return index
    End Function

    Private Sub ActualizarDgvParticiones()

        ' Limpiar filas de control dgvParticiones
        dgvParticiones.Rows.Clear()

        ' Agregar particiones libres a control dgvParticiones
        For Each p As Particion In ParticionesLibres
            dgvParticiones.Rows.Add(New String() {p.Numero, p.Direccion, p.Tamaño, "-", "-", p.Estado.ToString})
        Next

        ' Agregar particiones ocupadas a control dgvParticiones
        For Each p As Particion In ParticionesOcupadas
            dgvParticiones.Rows.Add(New String() {p.Numero, p.Direccion, p.Tamaño, p.Proceso.Nombre, p.Proceso.Tamaño, p.Estado.ToString})
        Next

        ' Ordenar lista de particiones en control dgvParticiones
        dgvParticiones.Sort(dgvParticiones.Columns(1), System.ComponentModel.ListSortDirection.Ascending)
        Dim c As Integer = 1
        For Each row As DataGridViewRow In dgvParticiones.Rows
            row.Cells(0).Value = c
            c += 1
        Next

    End Sub

    Private Sub ActualizaDgvProcesos()
        dgvProcesos.Rows.Clear()
        For Each p As Proceso In ListaProcesos
            dgvProcesos.Rows.Add(New String() {p.Nombre, p.Tamaño, p.Estado.ToString})
        Next
    End Sub

#End Region

#Region " Windows Form Events "

    Private Sub ParticionFija_Load(sender As Object, e As EventArgs) Handles Me.Load

        'Inicializando cola de procesos de prueba
        ListaProcesos = New List(Of Proceso)
        ListaProcesos.Add(New Proceso("J1", 10, EstadoProceso.Espera, 10)) ' J1 ~ 10kb
        ListaProcesos.Add(New Proceso("J2", 20, EstadoProceso.Espera, 20)) ' J2 ~ 20kb

        ' Actualizando DataGridView Particiones
        ActualizarDgvParticiones()

        ' Actualizando DataGridView Procesos
        ActualizaDgvProcesos()

        ' Inicializando Timer
        trProcesos.Interval = 1000
        trProcesos.Start()

    End Sub

    Private Sub btnIniciar_Click(sender As Object, e As EventArgs) Handles btnEjecutar.Click
        Ejecucion = True
        btnEjecutar.Visible = False
        AsignarEspacio()
    End Sub

    Private Sub btnAgregarProceso_Click(sender As Object, e As EventArgs) Handles btnAgregarProceso.Click

        ' Declarando variables
        Dim tamañoProcesoStr As String = String.Empty
        Dim duracionProcesoStr As String = String.Empty
        Dim tamañoProceso As Integer = 0
        Dim duracionProceso As Integer = 0

        ' Obteniendo tamaño de proceso
        tamañoProcesoStr = InputBox("Ingrese tamaño de proceso...", "Nuevo Proceso", "")
        tamañoProceso = IIf(String.IsNullOrEmpty(tamañoProcesoStr), 0, CInt(tamañoProcesoStr))
        duracionProcesoStr = InputBox("Ingrese duración de proceso...", "Nuevo Proceso", "")
        duracionProceso = IIf(String.IsNullOrEmpty(duracionProcesoStr), 0, CInt(duracionProcesoStr))

        ' Verificando si el tamaño del proceso es mayor que cero
        If tamañoProceso > 0 Then

            ' Declarando e Inicializando nuevo proceso
            Dim nuevoProceso As Proceso
            nuevoProceso = New Proceso("J" + CStr(ListaProcesos.Count + 1), tamañoProceso, EstadoProceso.Espera, duracionProceso)

            ' Agregando proceso a la lista de procesos
            ListaProcesos.Add(nuevoProceso)

            ' Realizando asignación 
            AsignarEspacio()

        Else ' de lo contrario
            MsgBox("El tamaño del proceso no puede estar vacio y debe ser mayor de cero.")
        End If

    End Sub

    Private Sub btnTerminarProceso_Click(sender As Object, e As EventArgs) Handles btnTerminarProceso.Click

        ' Verificando si se ha seleccionado un proceso 
        If dgvProcesos.SelectedRows.Count = 0 Then
            MsgBox("Debe seleccionar un proceso...")
            Exit Sub
        End If

        ' Verificando si el proceso seleccionado esta asignado a alguna particion
        If Not dgvProcesos.SelectedRows(0).Cells(2).Value = "Asignado" Then
            MsgBox("Debe seleccionar un proceso que este asignado a alguna particion.")
            Exit Sub
        End If

        ' Obteniendo nombre de proceso seleccionado
        Dim nomProcSel As String = String.Empty
        nomProcSel = dgvProcesos.SelectedRows(0).Cells(0).Value



    End Sub

    Private Sub btnCerrar_Click(sender As Object, e As EventArgs) Handles btnCerrar.Click
        Close()
    End Sub

    Private Sub trProcesos_Tick(sender As Object, e As EventArgs) Handles trProcesos.Tick

        ' Recorriendo lista de procesos
        For Each proceso As Proceso In ListaProcesos

            ' Verificando que el proceso se encuentre asignado 
            If proceso.Estado = EstadoProceso.Asignado Then

                ' Declarando variables
                Dim duracion As Integer = proceso.Duracion
                Dim horaInicio As DateTime = proceso.HoraInicio
                Dim horaActual As DateTime = Date.Now()
                Dim intervalo As Integer = horaActual.Subtract(horaInicio).Seconds

                ' Terminar proceso si alcanzo tiempo maximo de ejecucion 
                If (duracion - intervalo) <= 0 Then
                    TerminarProceso(proceso.Nombre)
                End If

            End If
        Next

    End Sub

    Private Sub btnInfo_Click(sender As Object, e As EventArgs) Handles btnInfo.Click

        ' Verificando si se ha seleccionado un proceso 
        If dgvProcesos.SelectedRows.Count = 0 Then
            MsgBox("Debe seleccionar un proceso...")
            Exit Sub
        End If

        ' Obteniendo nombre de proceso seleccionado
        Dim nomProcSel As String = String.Empty
        Dim procSel As Proceso = Nothing

        nomProcSel = dgvProcesos.SelectedRows(0).Cells(0).Value

        For Each proceso As Proceso In ListaProcesos
            If nomProcSel = proceso.Nombre Then
                procSel = proceso
            End If
        Next

        ' Mostrando informacion de proceso
        Dim infoProceso As InfoProceso = New InfoProceso()
        infoProceso.Inicializar(procSel, TamañoMemoria)
        infoProceso.ShowDialog()

    End Sub


#End Region

End Class