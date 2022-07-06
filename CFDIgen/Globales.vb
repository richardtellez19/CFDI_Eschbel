Imports System.Data.SqlClient

Module Globales
    Public Datos As New BD(GetSetting("MM", "DATABASE", "CONNSTR", "Sin establecer..."), GetSetting("MM", "Datos", "RutaDocs", "Sin establecer..."))
    'Public UsuarioActual As New Usuario
    Public TestFlag As Boolean = CBool(GetSetting("MM", "Datos", "Modo", "1"))

    'Public ConnString As String = "Data Source=C:\Users\Javier\Documents\Visual Studio 2008\Projects\Mobile-CBB\Mobile-CBB\MyDatabase#1.sdf; Password=mobilemetriks"

    Public rdrCSD As SqlDataReader
    Public rdrEmisor As SqlDataReader
    Public rdrFactRel As SqlDataReader
    Public rdrReceptor As SqlDataReader
    Public rdrFactGral As SqlDataReader
    Public rdrFactItems As DataTable
    Public rdrFactPagadas As DataTable
    Public rdrBanco As SqlDataReader
    Public TotalFactura = 0.0
    Public Doc As System.Xml.XmlDocument
    Public Tras As Boolean = False
    Public UTF8withoutBOM As System.Text.Encoding = New System.Text.UTF8Encoding(False)
    Public validSchema As Boolean = True
    Public CadenaOriginal_TFD As String = ""
    Public Empresa As String = ""
    Public reciboPago As Boolean = False
    Public Retencion As Boolean = False

    Public Enum pagos
        activo
        tipo
        cantidadPagada
        cantidadRestante
        folioPago
        fechaInicial
        fechaFinal
        rfcEmisor
    End Enum

    Public Function ImporteConLetra(ByVal Importe As String, ByVal Moneda As String, ByVal Terminacion As String) As String
        Importe = Replace(FormatCurrency(CDbl(IIf(IsNumeric(Importe), Importe, 0)), 2, TriState.True, TriState.False, TriState.False), "$", "")
        Dim arrImp() As String = Split(Importe, ".")
        Dim Entero As Integer = CInt(IIf(IsNumeric(arrImp(0)), arrImp(0), "0"))
        Dim Signo As String = IIf(Importe < 0, "MENOS ", "")
        Dim Letras As String = Signo & Num2Text(Math.Abs(Entero)) & " " & Moneda & " " & arrImp(1) & "/100 " & Terminacion

        Return Letras
    End Function

    Private Function Num2Text(ByVal value As Double) As String
        Select Case value
            Case 0 : Num2Text = "CERO"
            Case 1 : Num2Text = "UN"
            Case 2 : Num2Text = "DOS"
            Case 3 : Num2Text = "TRES"
            Case 4 : Num2Text = "CUATRO"
            Case 5 : Num2Text = "CINCO"
            Case 6 : Num2Text = "SEIS"
            Case 7 : Num2Text = "SIETE"
            Case 8 : Num2Text = "OCHO"
            Case 9 : Num2Text = "NUEVE"
            Case 10 : Num2Text = "DIEZ"
            Case 11 : Num2Text = "ONCE"
            Case 12 : Num2Text = "DOCE"
            Case 13 : Num2Text = "TRECE"
            Case 14 : Num2Text = "CATORCE"
            Case 15 : Num2Text = "QUINCE"
            Case Is < 20 : Num2Text = "DIECI" & Num2Text(value - 10)
            Case 20 : Num2Text = "VEINTE"
            Case Is < 30 : Num2Text = "VEINTI" & Num2Text(value - 20)
            Case 30 : Num2Text = "TREINTA"
            Case 40 : Num2Text = "CUARENTA"
            Case 50 : Num2Text = "CINCUENTA"
            Case 60 : Num2Text = "SESENTA"
            Case 70 : Num2Text = "SETENTA"
            Case 80 : Num2Text = "OCHENTA"
            Case 90 : Num2Text = "NOVENTA"
            Case Is < 100 : Num2Text = Num2Text(Int(value \ 10) * 10) & " Y " & Num2Text(value Mod 10)
            Case 100 : Num2Text = "CIEN"
            Case Is < 200 : Num2Text = "CIENTO " & Num2Text(value - 100)
            Case 200, 300, 400, 600, 800 : Num2Text = Num2Text(Int(value \ 100)) & "CIENTOS"
            Case 500 : Num2Text = "QUINIENTOS"
            Case 700 : Num2Text = "SETECIENTOS"
            Case 900 : Num2Text = "NOVECIENTOS"
            Case Is < 1000 : Num2Text = Num2Text(Int(value \ 100) * 100) & " " & Num2Text(value Mod 100)
            Case 1000 : Num2Text = "MIL"
            Case Is < 2000 : Num2Text = "MIL " & Num2Text(value Mod 1000)
            Case Is < 1000000 : Num2Text = Num2Text(Int(value \ 1000)) & " MIL"
                If value Mod 1000 Then Num2Text = Num2Text & " " & Num2Text(value Mod 1000)
            Case 1000000 : Num2Text = "UN MILLON"
            Case Is < 2000000 : Num2Text = "UN MILLON " & Num2Text(value Mod 1000000)
            Case Is < 1000000000000.0# : Num2Text = Num2Text(Int(value / 1000000)) & " MILLONES "
                If (value - Int(value / 1000000) * 1000000) Then Num2Text = Num2Text & " " & Num2Text(value - Int(value / 1000000) * 1000000)
            Case 1000000000000.0# : Num2Text = "UN BILLON"
            Case Is < 2000000000000.0# : Num2Text = "UN BILLON " & Num2Text(value - Int(value / 1000000000000.0#) * 1000000000000.0#)
            Case Else : Num2Text = Num2Text(Int(value / 1000000000000.0#)) & " BILLONES"
                If (value - Int(value / 1000000000000.0#) * 1000000000000.0#) Then Num2Text = Num2Text & " " & Num2Text(value - Int(value / 1000000000000.0#) * 1000000000000.0#)
        End Select

    End Function
End Module
