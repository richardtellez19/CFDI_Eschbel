Imports System.Data.SqlClient
Imports Scripting
Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Collections.Generic
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.Xsl
Imports System.Xml.XPath
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Xml.Schema
Imports System.Net.Mail
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports Ionic.Zip
Imports System.Text.RegularExpressions

Module ModuleMain
    Sub Main(ByVal args As String())
        Dim Comando As String = ""
        'args = {"7", "MOB100617FNA", "pruebas"}

        'UploadFTP("C:\Users\JAVIER\Dropbox\Visual Studio 2010\Projects\Mobile-SAF\Documentos\MOB100617FNA\CF-CFDI\C&A970530UQ2_B184.xml")

        Console.WriteLine("----------- MOBILE-METRIKS GENERADOR DE CFDi--------------")
        If TestFlag Then
            Console.WriteLine("*** ¡Modo de Pruebas! ***")
        End If

        Try
            Comando = args(0)
        Catch ex As Exception
            Console.WriteLine("Sin argumentos!" & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
            Exit Sub
        End Try

        Select Case Comando
            Case "0"    ' Entrar a menu compañia
                MenuCompania()
            Case "1"    ' Entrar a menu de configuracion
                MenuConfig()
            Case "2"    ' Generar CFDi
                GeneraCFDI(args(1), args(2))
            Case "3"    '  Cancelar CFDi
                Try
                    CancelaCFDI(args(1), args(2), args(3), args(4), 1)
                Catch ex As Exception
                    CancelaCFDI(args(1), args(2), args(3), 0, 1)
                End Try
            Case "4"    ' Reenviar CFDi
                ReenviarCFDI(args(1), args(2))
            Case "5"    ' Ver CFDI
                VerCFDI(args(1), args(2))
            Case "6"
                StatusFolios(args(1))
            Case "7"
                RecuperaCFDI()
            Case "9"
                GeneraCatCtasSAT(args(1), args(2), args(3))
            Case "10"
                GeneraBalanzaSAT(args(1), args(2), args(3), args(4), args(5), args(6))
            Case "11"
                GeneraPolizasSAT(args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9))
            Case "12"
                GeneraRepAuxFolSAT(args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9))
            Case "13"
                GeneraRepAuxCtasSAT(args(1), args(2), args(3), args(4), args(5), args(6), args(7), args(8), args(9))
            Case "23"    ' Consulta Status
                CancelaCFDI(args(1), args(2), 0, 0, 2)
            Case "33"   ' Generar CFDi 3.3
                GeneraCFDI3_3(args(1), args(2))
            Case "34"
                GeneraRecibo3_3(args(1), args(2))
        End Select

    End Sub

    Private Sub GeneraRepAuxCtasSAT(ByVal Emp As String, ByVal Anio As String, ByVal Periodo As String, ByVal AnioId As String, ByVal FechaIni As String, ByVal FechaFin As String, ByVal TipoSolicitud As String, ByVal NumOrden As String, ByVal NumTramite As String)
        Empresa = Emp

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT VATNUM " &
                                            "FROM " & Empresa & ".dbo.COMPDATA " &
                                            "WHERE COMP=-1")

        Dim FechaBalIni As Integer = Datos.GetDataScalar("SELECT SDATE-1440 FROM " & Empresa & ".dbo.GLPERIODS WHERE GL = " & AnioId & " AND PERIOD = " & Periodo & "")
        Dim FechaBalFin As Integer = Datos.GetDataScalar("SELECT EDATE FROM " & Empresa & ".dbo.GLPERIODS WHERE GL = " & AnioId & " AND PERIOD = " & Periodo & "")
        Dim query As String = "SELECT * " &
                                "FROM " &
                                "	(SELECT ACCOUNTS.ACCNAME, " &
                                "		ACCOUNTS.GLOB_ACCNATURE, " &
                                "		SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalIni & " THEN FNCBAL.BALANCE1*-1 ELSE 0.0 END) AS BALINICIAL, " &
                                "		SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.DEBIT1 ELSE 0.0 END) AS DEBITO, " &
                                "		SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.CREDIT1 ELSE 0.0 END) AS CREDITO, " &
                                "		SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.BALANCE1*-1 ELSE 0.0 END) AS BALFINAL " &
                                "	FROM " & Empresa & ".dbo.ACCOUNTS, " & Empresa & ".dbo.FNCBAL " &
                                "	WHERE ACCOUNTS.ACCOUNT <> 0 " &
                                "		AND ACCOUNTS.ACCOUNT   =  FNCBAL.ACCOUNT " &
                                "		AND ACCOUNTS.TMPFLAG  <> 'Y' " &
                                "		AND FNCBAL.GL = " & AnioId & " " &
                                "		AND (FNCBAL.CURDATE BETWEEN " & FechaBalIni & " " &
                                "		AND " & FechaBalFin & ") " &
                                "		AND ACCOUNTS.COMPANY  = -1 " &
                                "	GROUP BY ACCOUNTS.ACCNAME, ACCOUNTS.GLOB_ACCNATURE " &
                                "	HAVING SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.DEBIT1 ELSE 0.0 END)  <> 0.0 OR " &
                                "	SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.CREDIT1 ELSE 0.0 END) <> 0.0 OR " &
                                "	SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & "  THEN FNCBAL.BALANCE1*-1 ELSE 0.0 END) <> 0.0) AS BALANZA " &
                                "JOIN " &
                                "	(SELECT " &
                                "		ACCOUNTS.ACCNAME, " &
                                "		ACCOUNTS.ACCDES , " &
                                "		CAST(DATEADD(DAY, FNCITEMS.BALDATE/1440, '1988-01-01') AS DATE) AS BALDATE, " &
                                "		FNCTRANS.FNCNUM , " &
                                "		SUBSTRING( CASE WHEN ( ( COALESCE( FNCITEMSA.DETAILS , '' ) <> '' ) ) THEN ( COALESCE( FNCITEMSA.DETAILS , '' ) ) ELSE ( COALESCE( FNCTRANS.DETAILS , '' ) ) END + '                        ' , 1, 24) AS DETAILS, " &
                                "		CONVERT(DECIMAL(21,2), FNCITEMS.DEBIT1) AS DEBIT1, " &
                                "		CONVERT(DECIMAL(21,2), FNCITEMS.CREDIT1) AS CREDIT1, " &
                                "		CURRENCIES.CODE  AS MONEDA, " &
                                "		COALESCE( FNCITEMS.LINE , 0 ) AS LINE " &
                                "	FROM " & Empresa & ".dbo.ACCOUNTS " &
                                "	INNER JOIN " & Empresa & ".dbo.FNCBAL  ON ( FNCBAL.ACCOUNT = ACCOUNTS.ACCOUNT ) AND ( FNCBAL.CURDATE = " & FechaBalIni & " ) " &
                                "		AND ( FNCBAL.GL = " & AnioId & " ) AND ( ACCOUNTS.TMPFLAG <> 'Y' ) AND ( ( 0 <> 2 ) OR ( ( - ( 300 ) > ACCOUNTS.SECTION ) OR ( ACCOUNTS.SECTION > - ( 1 ) ) ) ) " &
                                "		AND ( ACCOUNTS.SECTION <= CASE WHEN ( ( ( 0 = 1 ) OR ( 0 = 2 ) ) ) THEN ( 9999999 ) WHEN ( ( 0 = 3 ) ) THEN ( - ( 100 ) ) ELSE ( - ( 1 ) ) END ) " &
                                "		AND ( ACCOUNTS.SECTION >= CASE WHEN ( ( ( 0 = 1 ) OR ( 0 = 2 ) ) ) THEN ( - ( 9999 ) ) WHEN ( ( 0 = 3 ) ) THEN ( - ( 300 ) ) ELSE ( - ( 110 ) ) END ) " &
                                "	INNER JOIN " & Empresa & ".dbo.CURRENCIES  ON ( CURRENCIES.CURRENCY = ACCOUNTS.CURRENCY ) " &
                                "	LEFT OUTER JOIN " & Empresa & ".dbo.FNCITEMS  ON FNCITEMS.BALDATE <= " & FechaBalFin & "  AND FNCITEMS.BALDATE >= " & FechaBalIni & " " &
                                "		AND ( COALESCE( FNCITEMS.GL , 0 ) > CASE WHEN ( ( '' = 'Y' ) ) THEN ( - ( 2 ) ) ELSE ( 0 ) END ) " &
                                "		AND ( FNCITEMS.ACCOUNT = ACCOUNTS.ACCOUNT ) AND ( COALESCE( FNCITEMS.FINAL , '' ) <> CASE WHEN ( ( '' = 'Y' ) ) THEN ( 'A' ) ELSE ( '' ) END ) " &
                                "		AND ( COALESCE( FNCITEMS.ENDYEARFLAG , '' ) <> 'Y' ) " &
                                "	LEFT OUTER JOIN " & Empresa & ".dbo.FNCTRANS  ON ( FNCTRANS.FNCTRANS = FNCITEMS.FNCTRANS ) " &
                                "	LEFT OUTER JOIN " & Empresa & ".dbo.FNCITEMSA  ON ( FNCITEMSA.FNCTRANS = FNCITEMS.FNCTRANS ) " &
                                "		AND ( FNCITEMSA.KLINE = FNCITEMS.KLINE ) " &
                                "	WHERE ( ( COALESCE( FNCITEMSA.FNCTRANS , 0 ) + ABS ( CONVERT(DECIMAL(21,2), FNCBAL.BALANCE3) ) ) <> 0.000000000 ) " &
                                "		AND ( COALESCE( FNCTRANS.GL , 0 ) >= CASE WHEN ( ( '' = 'Y' ) ) THEN ( - ( 1 ) ) ELSE ( 0 ) END ) AND 1 = 1 AND ( 1 = 1 )) AS AUXILIAR " &
                                "ON (BALANZA.ACCNAME=AUXILIAR.ACCNAME) " &
                                "ORDER BY BALANZA.ACCNAME ASC"
        'Console.WriteLine(query)
        Dim daAuxiliarCtas As New SqlDataAdapter(query, Datos.ConnectionString)


        rdrCSD.Read()
        rdrEmisor.Read()

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine("Generando informe de Reporte Auxiliar de Cuentas para la empresa: " & rdrEmisor("VATNUM").ToString.Trim)

            Dim Fecha As Date = Format(Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now), "yyyy-MM-ddTHH:mm:ss")
            Doc = New XmlDocument()
            Dim Atributo As XmlAttribute
            Dim dtAuxiliarCtas As New DataTable
            daAuxiliarCtas.Fill(dtAuxiliarCtas)

            Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?>" &
                        "<AuxiliarCtas:AuxiliarCtas xsi:schemaLocation=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas/AuxiliarCtas_1_3.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:AuxiliarCtas=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas"" " &
                        "Version=""1.3"" RFC="""" Mes="""" Anio="""" TipoSolicitud="""" > </AuxiliarCtas:AuxiliarCtas>")
            Dim AuxiliarCtas As XmlNode = Doc.GetElementsByTagName("AuxiliarCtas", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas")(0)

            AuxiliarCtas.Attributes("RFC").Value = rdrEmisor("VATNUM")
            AuxiliarCtas.Attributes("Mes").Value = Periodo.PadLeft(2, "0")
            AuxiliarCtas.Attributes("Anio").Value = CInt(Anio)

            Select Case TipoSolicitud
                Case "1"
                    TipoSolicitud = "AF"
                Case "2"
                    TipoSolicitud = "FC"
                Case "3"
                    TipoSolicitud = "DE"
                Case "4"
                    TipoSolicitud = "CO"
            End Select

            AuxiliarCtas.Attributes("TipoSolicitud").Value = TipoSolicitud

            If TipoSolicitud = "AF" Or TipoSolicitud = "FC" Then

                Atributo = Doc.CreateAttribute("NumOrden")
                Atributo.Value = NumOrden
                AuxiliarCtas.Attributes.Append(Atributo)
            End If

            If TipoSolicitud = "DE" Or TipoSolicitud = "CO" Then

                Atributo = Doc.CreateAttribute("NumTramite")
                Atributo.Value = NumTramite
                AuxiliarCtas.Attributes.Append(Atributo)
            End If

            Dim Cuenta As XmlNode = Nothing
            Try
                Dim CtaAnterior As String = ""
                For Each FilaCuenta As DataRow In dtAuxiliarCtas.Rows
                    If FilaCuenta("FNCNUM") <> CtaAnterior Then
                        If Not IsNothing(Cuenta) Then AuxiliarCtas.AppendChild(Cuenta)

                        'Console.WriteLine(FilaCuenta("FNCNUM"))
                        Cuenta = Doc.CreateElement("AuxiliarCtas", "Cuenta", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas")

                        Atributo = Doc.CreateAttribute("NumCta")
                        Atributo.Value = FilaCuenta("ACCNAME")
                        Cuenta.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("DesCta")
                        Atributo.Value = FilaCuenta("ACCDES")
                        Cuenta.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("SaldoIni")
                        Atributo.Value = IIf(FilaCuenta("GLOB_ACCNATURE") = "A", -1 * FilaCuenta("BALINICIAL"), FilaCuenta("BALINICIAL"))
                        Cuenta.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("SaldoFin")
                        Atributo.Value = IIf(FilaCuenta("GLOB_ACCNATURE") = "A", -1 * FilaCuenta("BALFINAL"), FilaCuenta("BALFINAL"))
                        Cuenta.Attributes.Append(Atributo)
                    End If

                    CtaAnterior = FilaCuenta("FNCNUM")

                    Dim DetalleAux As XmlNode = Doc.CreateElement("AuxiliarCtas", "DetalleAux", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas")

                    Atributo = Doc.CreateAttribute("Fecha")
                    Atributo.Value = Format(CDate(FilaCuenta("BALDATE")), "yyyy-MM-dd")
                    DetalleAux.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("NumUnIdenPol")
                    Atributo.Value = FilaCuenta("FNCNUM")
                    DetalleAux.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Concepto")
                    Atributo.Value = FilaCuenta("DETAILS")
                    DetalleAux.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Debe")
                    Atributo.Value = FilaCuenta("DEBIT1")
                    DetalleAux.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Haber")
                    Atributo.Value = FilaCuenta("CREDIT1")
                    DetalleAux.Attributes.Append(Atributo)

                    Cuenta.AppendChild(DetalleAux)
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Doc.AppendChild(AuxiliarCtas)

            Dim Params As New FirmaParams
            Params.ArchivoXSD = "AuxiliarCtas_1_3.xsd"
            Params.ArchivoXSLT = "AuxiliarCtas_1_2.xslt"
            Params.NodoCert = "AuxiliarCtas"
            Params.EspacioNombres = "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarCtas"
            Params.AtrCertif = "Certificado"
            Params.AtrNoCert = "noCertificado"
            Params.AtrSello = "Sello"

            Try
                Datos.Firma(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            Try
                Datos.Valida(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            If Not System.IO.Directory.Exists(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\AuxiliarCtas\") Then
                System.IO.Directory.CreateDirectory(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\AuxiliarCtas\")
            End If

            Dim RutaXML As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\AuxiliarCtas\" & rdrEmisor("VATNUM").ToString & Format(CInt(Anio), "0000") & Format(CInt(Periodo), "00") & "XC" & ".xml"
            Doc.Save(RutaXML)

            While 1
                Try
                    Dim xmlZIP As New ZipFile()
                    xmlZIP.AddFile(RutaXML, "")
                    xmlZIP.Save(RutaXML.Replace(".xml", ".zip"))
                    Console.WriteLine("")
                    Exit While
                Catch ex As Exception
                    Console.Write("*")
                    Exit Try
                End Try
            End While

            Console.WriteLine("¡Proceso concluido!")
            Console.Write("¿Desea abrir la carpeta? (S/N): ")

            If Console.ReadLine().ToUpper = "S" Then
                Shell("explorer.exe root = " & Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\AuxiliarCtas\", AppWinStyle.NormalFocus)
            End If
        Else
            Console.WriteLine("¡No se pudo generar el documento! Favor de verificar que tenga activada su cuenta." & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrCSD.Close()
        rdrEmisor.Close()
    End Sub

    Private Sub GeneraRepAuxFolSAT(ByVal Emp As String, ByVal Anio As String, ByVal Periodo As String, ByVal AnioId As String, ByVal FechaIni As String, ByVal FechaFin As String, ByVal TipoSolicitud As String, ByVal NumOrden As String, ByVal NumTramite As String)
        Empresa = Emp

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT VATNUM " &
                                            "FROM " & Empresa & ".dbo.COMPDATA " &
                                            "WHERE COMP=-1")

        Dim query As String = "SELECT " &
        "FNCTRANS.FNCNUM, " &
        "CAST(DATEADD(DAY, FNCTRANS.BALDATE/1440, '1988-01-01') AS DATE) AS BALDATE, " &
        "SUBSTRING( CASE WHEN ( ( FNCTRANS.SUP = 0 ) ) THEN ( '' ) WHEN ( ( COALESCE( SUPPLIERS.VATNUM , '' ) = '' ) ) THEN ( 'XAXX010101000' ) ELSE ( COALESCE( SUPPLIERS.VATNUM , '' ) ) END + 'XAXX010101000' , 1, 16) AS RFCPROV, " &
        "SUBSTRING( CASE WHEN ( ( FNCTRANS.CUST = 0 ) ) THEN ( '' ) WHEN ( ( COALESCE( CUSTOMERS.VATNUM , '' ) = '' ) ) THEN ( 'XAXX010101000' ) ELSE ( COALESCE( CUSTOMERS.VATNUM , '' ) ) END + 'XAXX010101000' , 1, 16) AS RFCCTE, " &
        "INVOICES.GLOB_FOLIOFISCAL, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), INVOICES.TOTPRICE) )) AS MONTO_FRA, " &
        "CURRENCIES1.CODE AS MONEDA_FRA, " &
        "(0.0 + ( (0.0+ ( CONVERT(DECIMAL(27,9), INVOICES.LEXCHANGE) * CURRENCIES1.EXCHQUANT ) ) )) AS TCAMB_FRA " &
        "FROM " & Empresa & ".dbo.GENLEDGERS " &
        "INNER JOIN " & Empresa & ".dbo.FNCTRANS  ON ( FNCTRANS.BALDATE <= " & FechaFin & " ) AND ( FNCTRANS.BALDATE >= " & FechaIni & " ) AND ( FNCTRANS.GL = GENLEDGERS.GL ) AND ( ( FNCTRANS.CUST <> 0 ) OR ( FNCTRANS.SUP <> 0 ) ) AND ( FNCTRANS.FINAL = 'Y' ) AND ( FNCTRANS.STORNOFLAG <> 'Z' ) AND ( FNCTRANS.STORNOFLAG <> 'Y' ) " &
        "INNER JOIN " & Empresa & ".dbo.INVOICES  ON ( INVOICES.GLOB_FOLIOFISCAL <> '' ) AND ( INVOICES.FNCTRANS = FNCTRANS.FNCTRANS ) " &
        "INNER JOIN " & Empresa & ".dbo.FNCPATTERNS  ON ( FNCPATTERNS.FNCPATTERN = FNCTRANS.FNCPATTERN ) " &
        "INNER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES1 ON ( CURRENCIES1.CURRENCY = INVOICES.CURRENCY ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CUSTOMERS  ON ( CUSTOMERS.CUST = FNCTRANS.CUST ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.SUPPLIERS  ON ( SUPPLIERS.SUP = FNCTRANS.SUP ) " &
        "WHERE ( ( FNCPATTERNS.FNCPATNAME = 'INV' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'MSH' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'GRV' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'CRD' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'VND' ) OR ( FNCPATTERNS.FNCPATNAME = 'CRV' ) ) ) ) ) ) AND ( GENLEDGERS.GLNAME = " & Anio & " ) AND ( 1 = 1 ) " &
        "ORDER BY 1"

        'Console.WriteLine(query)

        Dim daRepAuxFol As New SqlDataAdapter(query, Datos.ConnectionString)

        rdrCSD.Read()
        rdrEmisor.Read()

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine("Generando Reporte Auxiliar de Folios para la empresa: " & rdrEmisor("VATNUM").ToString.Trim)

            Dim Fecha As Date = Format(Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now), "yyyy-MM-ddTHH:mm:ss")
            Doc = New XmlDocument()
            Dim Atributo As XmlAttribute
            Dim dtRepAuxFol As New DataTable

            daRepAuxFol.Fill(dtRepAuxFol)

            Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?>" &
                        "<RepAux:RepAuxFol xsi:schemaLocation=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios/AuxiliarFolios_1_3.xslt"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:RepAux=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios"" " &
                        "Version=""1.3"" RFC="""" Mes="""" Anio="""" TipoSolicitud="""" > </RepAux:RepAuxFol>")
            Dim RepAuxFol As XmlNode = Doc.GetElementsByTagName("RepAuxFol", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios")(0)

            RepAuxFol.Attributes("RFC").Value = rdrEmisor("VATNUM")
            RepAuxFol.Attributes("Mes").Value = Periodo.PadLeft(2, "0")
            RepAuxFol.Attributes("Anio").Value = CInt(Anio)

            Select Case TipoSolicitud
                Case "1"
                    TipoSolicitud = "AF"
                Case "2"
                    TipoSolicitud = "FC"
                Case "3"
                    TipoSolicitud = "DE"
                Case "4"
                    TipoSolicitud = "CO"
            End Select

            RepAuxFol.Attributes("TipoSolicitud").Value = TipoSolicitud

            If TipoSolicitud = "AF" Or TipoSolicitud = "FC" Then
                Atributo = Doc.CreateAttribute("NumOrden")
                Atributo.Value = NumOrden
                RepAuxFol.Attributes.Append(Atributo)
            End If

            If TipoSolicitud = "DE" Or TipoSolicitud = "CO" Then
                Atributo = Doc.CreateAttribute("NumTramite")
                Atributo.Value = NumTramite
                RepAuxFol.Attributes.Append(Atributo)
            End If

            'Ciclo FOR para insertar el Detalle de los Folios
            Dim DetAuxFol As XmlNode = Nothing
            Try
                Dim PolizaAnterior As String = ""
                For Each FilaPoliza As DataRow In dtRepAuxFol.Rows
                    If FilaPoliza("FNCNUM") <> PolizaAnterior Then
                        If Not IsNothing(DetAuxFol) Then RepAuxFol.AppendChild(DetAuxFol)

                        'Console.WriteLine(FilaPoliza("FNCNUM"))
                        DetAuxFol = Doc.CreateElement("RepAux", "DetAuxFol", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios")

                        Atributo = Doc.CreateAttribute("NumUnIdenPol")
                        Atributo.Value = FilaPoliza("FNCNUM")
                        DetAuxFol.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Fecha")
                        Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                        DetAuxFol.Attributes.Append(Atributo)
                    End If

                    PolizaAnterior = FilaPoliza("FNCNUM")

                    'se pone nodo CompNal
                    Dim CompNal As XmlNode = Doc.CreateElement("RepAux", "ComprNal", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios")

                    Atributo = Doc.CreateAttribute("UUID_CFDI")
                    Atributo.Value = FilaPoliza("GLOB_FOLIOFISCAL")
                    CompNal.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("RFC")
                    Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                    CompNal.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("MontoTotal")
                    Atributo.Value = FilaPoliza("MONTO_FRA")
                    CompNal.Attributes.Append(Atributo)

                    If FilaPoliza("MONEDA_FRA") <> "MXN" Then
                        Atributo = Doc.CreateAttribute("Moneda")
                        Atributo.Value = FilaPoliza("MONEDA_FRA")
                        CompNal.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("TipCamb")
                        Atributo.Value = FilaPoliza("TCAMB_FRA")
                        CompNal.Attributes.Append(Atributo)
                    End If

                    DetAuxFol.AppendChild(CompNal)
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            If Not IsNothing(DetAuxFol) Then RepAuxFol.AppendChild(DetAuxFol)

            Dim Params As New FirmaParams
            Params.ArchivoXSD = "AuxiliarFolios_1_3.xsd"
            Params.ArchivoXSLT = "AuxiliarFolios_1_2.xslt"
            Params.NodoCert = "RepAuxFol"
            Params.EspacioNombres = "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/AuxiliarFolios"
            Params.AtrCertif = "Certificado"
            Params.AtrNoCert = "noCertificado"
            Params.AtrSello = "Sello"

            Try
                Datos.Firma(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            Try
                Datos.Valida(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            If Not System.IO.Directory.Exists(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\RepAuxFol\") Then
                System.IO.Directory.CreateDirectory(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\RepAuxFol\")
            End If

            Dim RutaXML As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\RepAuxFol\" & rdrEmisor("VATNUM").ToString & Format(CInt(Anio), "0000") & Format(CInt(Periodo), "00") & "XF" & ".xml"
            Doc.Save(RutaXML)

            While 1
                Try
                    Dim xmlZIP As New ZipFile()
                    xmlZIP.AddFile(RutaXML, "")
                    xmlZIP.Save(RutaXML.Replace(".xml", ".zip"))
                    Console.WriteLine("")
                    Exit While
                Catch ex As Exception
                    Console.Write("*")
                    Exit Try
                End Try
            End While

            Console.WriteLine("¡Proceso concluido!")
            Console.Write("¿Desea abrir la carpeta? (S/N): ")

            If Console.ReadLine().ToUpper = "S" Then
                Shell("explorer.exe root = " & Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\RepAuxFol\", AppWinStyle.NormalFocus)
            End If
        Else
            Console.WriteLine("¡No se pudo generar el documento! Favor de verificar que tenga activada su cuenta." & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrCSD.Close()
        rdrEmisor.Close()
    End Sub

    Private Sub GeneraPolizasSAT(ByVal Emp As String, ByVal Anio As String, ByVal Periodo As String, ByVal AnioId As String, ByVal FechaIni As String, ByVal FechaFin As String, ByVal TipoSolicitud As String, ByVal NumOrden As String, ByVal NumTramite As String)
        Empresa = Emp

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT VATNUM " &
                                            "FROM " & Empresa & ".dbo.COMPDATA " &
                                            "WHERE COMP=-1")

        Dim queryRecibos As String = "SELECT FNCTRANS.FNCNUM ,  " &
        "CAST(DATEADD(DAY, FNCTRANS.BALDATE/1440, '1988-01-01') AS DATE) AS BALDATE,  " &
        "FNCTRANS.DETAILS ,  " &
        "ACCOUNTS.ACCNAME ,  " &
        "ACCOUNTS.ACCDES, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT1) )) AS DEBIT1, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT1) )) AS CREDIT1,  " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT2) )) AS DEBIT2 ,  " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT2) )) AS CREDIT2,  " &
        "INVOICES.PAYDATE ,  " &
        "COALESCE( SUPPLIERS.SUPNAME , '' ) AS SUPNAME, " &
        "COALESCE( SUPPLIERS.VATNUM , '' ) AS RFCPROV,  " &
        "COALESCE( CUSTOMERS.CUSTNAME , '' ) AS CUSTNAME,  " &
        "COALESCE( CUSTOMERS.VATNUM, '' ) AS RFCCTE, " &
        "COALESCE( CURRENCIES6.CODE , '' ) AS MONEDA_PLZA, " &
        "(0.0 + ( (0.0+ ( CONVERT(DECIMAL(27,9), FNCTRANS.EXCHANGE2) * COALESCE( CURRENCIES6.EXCHQUANT , 0 ) ) ) )) AS TCAMB_PLZA, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCTRANS.SUM2) )) AS SUM2, " &
        "COALESCE( INVOICES1.IVNUM , '' ) AS IVNUM,  " &
        "COALESCE( INVOICES1.GLOB_FOLIOFISCAL , '' )  AS GLOB_FOLIOFISCAL, " &
        "(0.0 + ( COALESCE( CONVERT(DECIMAL(19,2), INVOICES1.TOTPRICE) , 0.0 ) )) AS MONTO_FRA, " &
        "CURRENCIES1.CODE AS MONEDA_FRA, " &
        "(0.0 + ( (0.0+ ( COALESCE( CONVERT(DECIMAL(27,9), INVOICES1.LEXCHANGE), 0.0 ) * CURRENCIES1.EXCHQUANT ) ) )) AS TCAMB_FRA,  " &
        "COALESCE( INVOICES1.BOOKNUM , '' ) AS BOOKNUM, " &
        "FNCITEMS.FRECONNUM " &
        "FROM " & Empresa & ".dbo.FNCPATTERNS  INNER JOIN  " & Empresa & ".dbo.GENLEDGERS  ON ( GENLEDGERS.GLNAME = '" & Anio & "' ) " &
        " INNER JOIN " & Empresa & ".dbo.FNCTRANS  ON ( FNCTRANS.BALDATE <= " & FechaFin & " ) AND ( FNCTRANS.BALDATE >= " & FechaIni & " ) AND ( FNCTRANS.FNCPATTERN = FNCPATTERNS.FNCPATTERN ) AND ( FNCTRANS.GL = GENLEDGERS.GL ) AND ( ( FNCTRANS.CUST <> 0 ) OR ( FNCTRANS.SUP <> 0 ) ) " &
        " INNER JOIN " & Empresa & ".dbo.INVOICES  ON ( INVOICES.FNCTRANS = FNCTRANS.FNCTRANS ) " &
        " INNER JOIN " & Empresa & ".dbo.FNCITEMS  ON ( FNCITEMS.FNCTRANS = FNCTRANS.FNCTRANS ) AND ( FNCITEMS.FRECONNUM <> - ( 1 ) ) " &
        " INNER JOIN " & Empresa & ".dbo.ACCOUNTS  ON ( ACCOUNTS.ACCOUNT = FNCITEMS.ACCOUNT ) " &
        " INNER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES1 ON 1 = 1 " &
        " LEFT OUTER JOIN " & Empresa & ".dbo.FNCITEMSA  ON ( FNCITEMSA.KLINE = FNCITEMS.KLINE ) AND ( FNCITEMSA.FNCTRANS = FNCITEMS.FNCTRANS ) " &
        " LEFT OUTER JOIN " & Empresa & ".dbo.INVOICES INVOICES1 ON ( INVOICES1.IV = COALESCE( FNCITEMSA.IV , 0 ) ) " &
        " LEFT OUTER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES6 ON ( CURRENCIES6.CURRENCY = FNCTRANS.CURRENCY3 ) " &
        " LEFT OUTER JOIN " & Empresa & ".dbo.CUSTOMERS  ON ( CUSTOMERS.CUST = FNCTRANS.CUST ) " &
        " LEFT OUTER JOIN " & Empresa & ".dbo.SUPPLIERS  ON ( SUPPLIERS.SUP = FNCTRANS.SUP ) " &
        "WHERE ( COALESCE( INVOICES1.CURRENCY , 0 ) = CURRENCIES1.CURRENCY ) AND ( FNCPATTERNS.FNCPATNAME = 'REC' ) AND ( 1 = 1 ) " &
        "ORDER BY 1 , 21 DESC"

        Dim queryTransferencias As String = "SELECT FNCTRANS.FNCNUM , " &
        "CAST(DATEADD(DAY, FNCTRANS.BALDATE/1440, '1988-01-01') AS DATE) AS BALDATE , " &
        "FNCTRANS.DETAILS , " &
        "ACCOUNTS.ACCNAME , " &
        "ACCOUNTS.ACCDES , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT1) )) AS DEBIT1 , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT1) )) AS CREDIT1, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT2) )) AS DEBIT2 , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT2) )) AS CREDIT2, " &
        "CASH.PAYACCOUNT , " &
        "BANKS5.BANKCODE , " &
        "BANKS5.BANKNAME , " &
        "PAYMENT.PAYACCOUNT AS PAYACCOUNT2 , " &
        "COALESCE( BANKS.BANKCODE , '' ) AS BANKCODE2, " &
        "COALESCE( BANKS.BANKNAME , '' ) AS BANKNAME2 , " &
        "CAST(DATEADD(DAY, INVOICES.PAYDATE/1440, '1988-01-01') AS DATE) AS PAYDATE , " &
        "COALESCE( SUPPLIERS.SUPNAME , '' ) AS SUPNAME, " &
        "COALESCE( SUPPLIERS.VATNUM , '' ) AS RFCPROV, " &
        "COALESCE( CUSTOMERS.CUSTNAME , '' ) AS CUSTNAME, " &
        "COALESCE( CUSTOMERS.VATNUM , '' ) AS RFCCTE, " &
        "COALESCE( CURRENCIES6.CODE , '' ) AS MONEDA_PLZA, " &
        "(0.0 + ( (0.0+ ( CONVERT(DECIMAL(27,9), FNCTRANS.EXCHANGE2) * COALESCE( CURRENCIES6.EXCHQUANT , 0 ) ) ) )) AS TCAMB_PLZA, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCTRANS.SUM2) ))  AS SUM2, " &
        "COALESCE( INVOICES1.IVNUM , '' ) AS IVNUM, " &
        "COALESCE( INVOICES1.GLOB_FOLIOFISCAL , '' ) AS GLOB_FOLIOFISCAL, " &
        "(0.0 + ( COALESCE( CONVERT(DECIMAL(19,2), INVOICES1.TOTPRICE) , 0.0 ) )) AS MONTO_FRA, " &
        "CURRENCIES1.CODE AS MONEDA_FRA , " &
        "(0.0 + ( (0.0+ ( COALESCE( CONVERT(DECIMAL(27,9), INVOICES1.LEXCHANGE) , 0.0 ) * CURRENCIES1.EXCHQUANT ) ) )) AS TCAMB_FRA, " &
        "FNCITEMS.FRECONNUM, COUNTRIES.COUNTRYNAME PAIS_PROV, " &
        "COALESCE( INVOICES1.BOOKNUM , '' ) AS BOOKNUM, " &
        "PCOUNTRIES.COUNTRYNAME AS PAIS_RETIRO, " &
        "DCOUNTRIES.COUNTRYNAME AS PAIS_DEPOSITO " &
        "FROM " & Empresa & ".dbo.GENLEDGERS " &
        "INNER JOIN " & Empresa & ".dbo.FNCTRANS  ON ( FNCTRANS.BALDATE <= " & FechaFin & " ) AND ( FNCTRANS.BALDATE >= " & FechaIni & " ) AND ( FNCTRANS.GL = GENLEDGERS.GL ) AND ( ( FNCTRANS.CUST <> 0 ) OR ( FNCTRANS.SUP <> 0 ) ) " &
        "INNER JOIN " & Empresa & ".dbo.PAYMENT  ON 1 = 1 " &
        "INNER JOIN " & Empresa & ".dbo.FNCPATTERNS  ON ( FNCPATTERNS.FNCPATTERN = FNCTRANS.FNCPATTERN ) " &
        "INNER JOIN " & Empresa & ".dbo.INVOICES  ON ( INVOICES.IV = PAYMENT.IV ) " &
        "INNER JOIN " & Empresa & ".dbo.CASH  ON ( CASH.CASH = INVOICES.CASH ) " &
        "INNER JOIN " & Empresa & ".dbo.FNCITEMS  ON ( FNCITEMS.FNCTRANS = FNCTRANS.FNCTRANS ) AND ( FNCITEMS.FRECONNUM <> - ( 1 ) ) " &
        "INNER JOIN " & Empresa & ".dbo.BANKS BANKS5 ON ( BANKS5.BANK = CASH.BANK ) " &
        "INNER JOIN " & Empresa & ".dbo.ACCOUNTS  ON ( ACCOUNTS.ACCOUNT = FNCITEMS.ACCOUNT ) " &
        "INNER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES1 ON 1 = 1 " &
        "INNER JOIN " & Empresa & ".dbo.BANKBRANCHES ON (BANKBRANCHES.BANK = BANKS5.BANK) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.FNCITEMSA  ON ( FNCITEMSA.KLINE = FNCITEMS.KLINE ) AND ( FNCITEMSA.FNCTRANS = FNCITEMS.FNCTRANS ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.INVOICES INVOICES1 ON ( INVOICES1.IV = COALESCE( FNCITEMSA.IV , 0 ) ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES6 ON ( CURRENCIES6.CURRENCY = FNCTRANS.CURRENCY3 ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CUSTOMERS  ON ( CUSTOMERS.CUST = FNCTRANS.CUST ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.SUPPLIERS  ON ( SUPPLIERS.SUP = FNCTRANS.SUP ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.COUNTRIES  ON (COUNTRIES.COUNTRY = SUPPLIERS.COUNTRY) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.BANKS  ON ( BANKS.BANK = PAYMENT.BANK ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.BANKBRANCHES BANKBRANCHES1 ON (BANKBRANCHES1.BANK = BANKS.BANK) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.COUNTRIES PCOUNTRIES ON (PCOUNTRIES.COUNTRY = BANKBRANCHES.COUNTRY) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.COUNTRIES DCOUNTRIES ON (DCOUNTRIES.COUNTRY = BANKBRANCHES1.COUNTRY) " &
        "WHERE ( COALESCE( INVOICES1.CURRENCY , 0 ) = CURRENCIES1.CURRENCY ) AND ( INVOICES.FNCTRANS = FNCTRANS.FNCTRANS ) AND FNCPATTERNS.FNCPATNAME LIKE 'TR%' AND ( GENLEDGERS.GLNAME = '" & Anio & "' ) AND ( 1 = 1 ) ORDER BY 1 , 27 DESC"

        Dim queryCheques As String = "SELECT FNCTRANS.FNCNUM , " &
        "CAST(DATEADD(DAY, FNCTRANS.BALDATE/1440, '1988-01-01') AS DATE) AS BALDATE , " &
        "FNCTRANS.DETAILS , " &
        "ACCOUNTS.ACCNAME , " &
        "ACCOUNTS.ACCDES , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT1) )) AS DEBIT1 , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT1) )) AS CREDIT1, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT2) )) AS DEBIT2 , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT2) )) AS CREDIT2, " &
        "CASH.PAYACCOUNT , " &
        "BANKS5.BANKCODE , " &
        "BANKS5.BANKNAME , " &
        "INVOICES.BOOKNUM , " &
        "INVOICES.TOTPRICE AS MONTO_CHEQUE , " &
        "CAST(DATEADD(DAY, INVOICES.PAYDATE/1440, '1988-01-01') AS DATE) AS PAYDATE , " &
        "COALESCE( SUPPLIERS.SUPNAME , '' ) AS SUPNAME, " &
        "COALESCE( SUPPLIERS.VATNUM , '' ) AS RFCPROV, " &
        "COALESCE( CUSTOMERS.CUSTNAME , '' ) AS CUSTNAME , " &
        "COALESCE( CUSTOMERS.VATNUM , '' ) AS RFCCTE , " &
        "COALESCE( CURRENCIES6.CODE , '' ) AS MONEDA_PLZA, " &
        "(0.0 + ( (0.0+ ( CONVERT(DECIMAL(27,9), FNCTRANS.EXCHANGE2) * COALESCE( CURRENCIES6.EXCHQUANT , 0 ) ) ) )) AS TCAMB_PLZA, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCTRANS.SUM2) )) AS SUM2, " &
        "COALESCE( INVOICES1.IVNUM , '' ) AS IVNUM, " &
        "COALESCE( INVOICES1.GLOB_FOLIOFISCAL , '' ) AS GLOB_FOLIOFISCAL, " &
        "(0.0 + ( COALESCE( CONVERT(DECIMAL(19,2), INVOICES1.TOTPRICE) , 0.0 ) )) AS MONTO_FRA, " &
        "CURRENCIES1.CODE AS MONEDA_FRA, " &
        "(0.0 + ( (0.0+ ( COALESCE( CONVERT(DECIMAL(27,9), INVOICES1.LEXCHANGE) , 0.0 ) * CURRENCIES1.EXCHQUANT ) ) )) AS TCAMB_FRA, " &
        "FNCITEMS.FRECONNUM, " &
        "INVOICES1.BOOKNUM, " &
        "COUNTRIES.COUNTRYNAME " &
        "FROM " & Empresa & ".dbo.GENLEDGERS " &
        "INNER JOIN " & Empresa & ".dbo.FNCTRANS  ON ( FNCTRANS.BALDATE <= " & FechaFin & " ) AND ( FNCTRANS.BALDATE >= " & FechaIni & " ) AND ( FNCTRANS.FINAL = 'Y' ) AND ( FNCTRANS.STORNOFLAG <> 'Y' ) AND ( FNCTRANS.GL = GENLEDGERS.GL ) AND ( ( FNCTRANS.CUST <> 0 ) OR ( FNCTRANS.SUP <> 0 ) ) " &
        "INNER JOIN " & Empresa & ".dbo.INVOICES  ON ( INVOICES.FNCTRANS = FNCTRANS.FNCTRANS ) " &
        "INNER JOIN " & Empresa & ".dbo.FNCPATTERNS  ON ( FNCPATTERNS.FNCPATTERN = FNCTRANS.FNCPATTERN ) " &
        "INNER JOIN " & Empresa & ".dbo.CASH  ON ( CASH.CASH = INVOICES.CASH ) " &
        "INNER JOIN " & Empresa & ".dbo.FNCITEMS  ON ( FNCITEMS.FNCTRANS = FNCTRANS.FNCTRANS ) AND ( FNCITEMS.FRECONNUM <> - ( 1 ) ) " &
        "INNER JOIN " & Empresa & ".dbo.ACCOUNTS  ON ( ACCOUNTS.ACCOUNT = FNCITEMS.ACCOUNT ) " &
        "INNER JOIN " & Empresa & ".dbo.BANKS BANKS5 ON ( BANKS5.BANK = CASH.BANK ) " &
        "INNER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES1 ON 1 = 1 " &
        "INNER JOIN " & Empresa & ".dbo.BANKBRANCHES ON (BANKBRANCHES.BANK = BANKS5.BANK) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.FNCITEMSA  ON ( FNCITEMSA.KLINE = FNCITEMS.KLINE ) AND ( FNCITEMSA.FNCTRANS = FNCITEMS.FNCTRANS ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.INVOICES INVOICES1 ON ( INVOICES1.IV = COALESCE( FNCITEMSA.IV , 0 ) ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES6 ON ( CURRENCIES6.CURRENCY = FNCTRANS.CURRENCY3 ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CUSTOMERS  ON ( CUSTOMERS.CUST = FNCTRANS.CUST ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.SUPPLIERS  ON ( SUPPLIERS.SUP = FNCTRANS.SUP ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.COUNTRIES ON (COUNTRIES.COUNTRY = BANKBRANCHES.BANK) " &
        "WHERE ( COALESCE( INVOICES1.CURRENCY , 0 ) = CURRENCIES1.CURRENCY ) AND FNCPATTERNS.FNCPATNAME LIKE 'CH%' AND " &
        "( GENLEDGERS.GLNAME = '" & Anio & "' ) AND ( 1 = 1 ) ORDER BY 1 , 25 DESC"

        Dim queryOtros As String = "SELECT FNCTRANS.FNCNUM , " &
        "CAST(DATEADD(DAY, FNCTRANS.BALDATE/1440, '1988-01-01') AS DATE) AS BALDATE , " &
        "FNCTRANS.DETAILS , " &
        "ACCOUNTS.ACCNAME , " &
        "ACCOUNTS.ACCDES , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT1) )) AS DEBIT1, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT1) )) AS CREDIT1, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.DEBIT2) )) AS DEBIT2 ,  " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCITEMS.CREDIT2) )) AS CREDIT2,  " &
        "CAST(DATEADD(DAY, INVOICES.PAYDATE/1440, '1988-01-01') AS DATE) AS PAYDATE , " &
        "COALESCE( SUPPLIERS.SUPNAME , '' ) AS SUPNAME , " &
        "COALESCE( SUPPLIERS.VATNUM , '' ) AS RFCPROV, " &
        "COALESCE( CUSTOMERS.CUSTNAME , '' ) AS CUSTNAME , " &
        "COALESCE( CUSTOMERS.VATNUM , '' ) AS RFCCTE, " &
        "COALESCE( CURRENCIES6.CODE , '' ) AS MONEDA_PLZA , " &
        "(0.0 + ( (0.0+ ( CONVERT(DECIMAL(27,9), FNCTRANS.EXCHANGE2) * COALESCE( CURRENCIES6.EXCHQUANT , 0 ) ) ) )) AS TCAMB_PLZA, " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), FNCTRANS.SUM2) )) AS SUM2, " &
        "INVOICES.IVNUM , " &
        "INVOICES.GLOB_FOLIOFISCAL , " &
        "(0.0 + ( CONVERT(DECIMAL(19,2), INVOICES.TOTPRICE) )) AS MONTO_FRA , " &
        "CURRENCIES1.CODE AS MONEDA_FRA , " &
        "(0.0 + ( (0.0+ ( CONVERT(DECIMAL(27,9), INVOICES.LEXCHANGE) * CURRENCIES1.EXCHQUANT ) ) )) AS TCAMB_FRA , " &
        "FNCITEMS.FRECONNUM " &
        "FROM " & Empresa & ".dbo.GENLEDGERS " &
        "INNER JOIN " & Empresa & ".dbo.FNCTRANS  ON ( FNCTRANS.BALDATE <= " & FechaFin & " ) AND ( FNCTRANS.BALDATE >= " & FechaIni & " ) AND ( FNCTRANS.GL = GENLEDGERS.GL ) AND ( ( FNCTRANS.CUST <> 0 ) OR ( FNCTRANS.SUP <> 0 ) ) " &
        "INNER JOIN " & Empresa & ".dbo.INVOICES  ON ( INVOICES.FNCTRANS = FNCTRANS.FNCTRANS ) " &
        "INNER JOIN " & Empresa & ".dbo.FNCITEMS  ON ( FNCITEMS.FNCTRANS = FNCTRANS.FNCTRANS ) AND ( FNCITEMS.FRECONNUM <> - ( 1 ) )   " &
        "INNER JOIN " & Empresa & ".dbo.ACCOUNTS  ON ( ACCOUNTS.ACCOUNT = FNCITEMS.ACCOUNT ) " &
        "INNER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES1 ON ( CURRENCIES1.CURRENCY = INVOICES.CURRENCY ) " &
        "INNER JOIN " & Empresa & ".dbo.FNCPATTERNS  ON ( FNCPATTERNS.FNCPATTERN = FNCTRANS.FNCPATTERN ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.FNCITEMSA  ON ( FNCITEMSA.KLINE = FNCITEMS.KLINE ) AND ( FNCITEMSA.FNCTRANS = FNCITEMS.FNCTRANS ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CURRENCIES CURRENCIES6 ON ( CURRENCIES6.CURRENCY = FNCTRANS.CURRENCY3 )   " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.CUSTOMERS  ON ( CUSTOMERS.CUST = FNCTRANS.CUST ) " &
        "LEFT OUTER JOIN " & Empresa & ".dbo.SUPPLIERS  ON ( SUPPLIERS.SUP = FNCTRANS.SUP ) " &
        "WHERE ( ( FNCPATTERNS.FNCPATNAME = 'INV' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'MSH' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'GRV' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'CRD' ) OR ( ( FNCPATTERNS.FNCPATNAME = 'VND' ) OR ( FNCPATTERNS.FNCPATNAME = 'CRV' ) OR ( FNCPATTERNS.FNCPATNAME = 'M' ) ) ) ) ) ) AND ( GENLEDGERS.GLNAME = '" & Anio & "' ) AND ( 1 = 1 ) ORDER BY 1 , 21 DESC"

        'Console.WriteLine(queryRecibos)
        'Console.WriteLine(queryTransferencias)
        'Console.WriteLine(queryCheques)
        'Console.WriteLine(queryOtros)

        Dim daPolizasRecibos As New SqlDataAdapter(queryRecibos, Datos.ConnectionString)
        Dim daPolizasTranfer As New SqlDataAdapter(queryTransferencias, Datos.ConnectionString)
        Dim daPolizasCheques As New SqlDataAdapter(queryCheques, Datos.ConnectionString)
        Dim daPolizasOtros As New SqlDataAdapter(queryOtros, Datos.ConnectionString)
        daPolizasRecibos.SelectCommand.CommandTimeout = 0
        daPolizasTranfer.SelectCommand.CommandTimeout = 0
        daPolizasCheques.SelectCommand.CommandTimeout = 0
        daPolizasOtros.SelectCommand.CommandTimeout = 0

        rdrCSD.Read()
        rdrEmisor.Read()

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine("Generando informe de Pólizas para la empresa: " & rdrEmisor("VATNUM").ToString.Trim)

            Dim Fecha As Date = Format(Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now), "yyyy-MM-ddTHH:mm:ss")
            Doc = New XmlDocument()
            Dim Atributo As XmlAttribute
            Dim dtPolizasRecibos As New DataTable
            Dim dtPolizasTransfer As New DataTable
            Dim dtPolizasCheques As New DataTable
            Dim dtPolizasOtros As New DataTable

            daPolizasRecibos.Fill(dtPolizasRecibos)
            daPolizasTranfer.Fill(dtPolizasTransfer)
            daPolizasCheques.Fill(dtPolizasCheques)
            daPolizasOtros.Fill(dtPolizasOtros)

            Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?>" &
                        "<PLZ:Polizas xsi:schemaLocation=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo/PolizasPeriodo_1_3.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:PLZ=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo"" " &
                        "Version=""1.3"" RFC="""" Mes="""" Anio="""" TipoSolicitud="""" > </PLZ:Polizas>")
            Dim Polizas As XmlNode = Doc.GetElementsByTagName("Polizas", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")(0)

            Polizas.Attributes("RFC").Value = rdrEmisor("VATNUM")
            Polizas.Attributes("Mes").Value = Periodo.PadLeft(2, "0")
            Polizas.Attributes("Anio").Value = CInt(Anio)

            'If Anio > Fecha.Year Then
            '    Console.WriteLine("No se puede generar el documento porque el Año ingresado es mayor al año actual")
            '    Console.ReadLine()
            '    Exit Sub
            'End If

            'Dim TipoSolicitud As String = ""
            Select Case TipoSolicitud
                Case "1"
                    TipoSolicitud = "AF"
                Case "2"
                    TipoSolicitud = "FC"
                Case "3"
                    TipoSolicitud = "DE"
                Case "4"
                    TipoSolicitud = "CO"
            End Select

            'Do
            '    Console.Write("Escriba ""AF"" - Acto de Fiscalización, ""FC"" - Fiscalización Compulsa, ""DE"" - Devolución, o ""CO"" - Compensación." & vbCrLf & "Tipo de Solicitud: ")
            '    TipoSolicitud = Console.ReadLine.Trim.ToUpper
            'Loop While (TipoSolicitud <> "AF" And TipoSolicitud <> "FC" And TipoSolicitud <> "DE" And TipoSolicitud <> "CO")

            Polizas.Attributes("TipoSolicitud").Value = TipoSolicitud

            If TipoSolicitud = "AF" Or TipoSolicitud = "FC" Then
                '    Dim NumOrden As String = ""

                '    Do
                '        Console.Write("Escriba las 13 dígitos del Número de Orden asignado al acto de fiscalización al que hace referencia la solicitud de la póliza." & vbCrLf & "No. Orden: ")
                '        NumOrden = Console.ReadLine.Trim.ToUpper
                '    Loop While (NumOrden <> "" And NumOrden.Length < 13)

                Atributo = Doc.CreateAttribute("NumOrden")
                Atributo.Value = NumOrden
                Polizas.Attributes.Append(Atributo)
            End If

            If TipoSolicitud = "DE" Or TipoSolicitud = "CO" Then
                '    Dim NumTramite As String = ""

                '    Do
                '        Console.Write("Escriba las 10 dígitos del Número de Trámite asignado a la solicitud de devolución o compensación al que hace referencia la solicitud de la póliza." & vbCrLf & "No. Orden: ")
                '        NumTramite = Console.ReadLine.Trim.ToUpper
                '    Loop While (NumTramite <> "" And NumTramite.Length < 10)

                Atributo = Doc.CreateAttribute("NumTramite")
                Atributo.Value = NumTramite
                Polizas.Attributes.Append(Atributo)
            End If

            'Ciclo FOR para insertar las Pólizas de Transferencias
            Dim Poliza As XmlNode = Nothing
            Try
                Console.WriteLine("Transferencias")
                Dim PolizaAnterior As String = ""
                For Each FilaPoliza As DataRow In dtPolizasTransfer.Rows
                    If FilaPoliza("FNCNUM") <> PolizaAnterior Then
                        If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)

                        'Console.WriteLine(FilaPoliza("FNCNUM"))
                        Poliza = Doc.CreateElement("PLZ", "Poliza", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("NumUnIdenPol")
                        Atributo.Value = FilaPoliza("FNCNUM")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Fecha")
                        Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Concepto")
                        Atributo.Value = FilaPoliza("DETAILS")
                        Poliza.Attributes.Append(Atributo)
                    End If

                    PolizaAnterior = FilaPoliza("FNCNUM")

                    Dim Transaccion As XmlNode = Doc.CreateElement("PLZ", "Transaccion", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = FilaPoliza("ACCNAME")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("DesCta")
                    Atributo.Value = FilaPoliza("ACCDES")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Concepto")
                    Atributo.Value = FilaPoliza("DETAILS")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Debe")
                    Atributo.Value = FilaPoliza("DEBIT1")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Haber")
                    Atributo.Value = FilaPoliza("CREDIT1")
                    Transaccion.Attributes.Append(Atributo)

                    If FilaPoliza("PAIS_PROV").ToString.ToUpper = "MEXICO" Or FilaPoliza("PAIS_PROV").ToString.ToUpper = "MÉXICO" Then
                        If FilaPoliza("GLOB_FOLIOFISCAL") <> "" And FilaPoliza("GLOB_FOLIOFISCAL") <> "000000000000000000000000000000000000" And FilaPoliza("GLOB_FOLIOFISCAL") <> "00000000-0000-0000-0000-000000000000" Then
                            'se pone nodo CompNal
                            Dim CompNal As XmlNode = Doc.CreateElement("PLZ", "CompNal", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("UUID_CFDI")
                            Atributo.Value = FilaPoliza("GLOB_FOLIOFISCAL")
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("RFC")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("MontoTotal")
                            Atributo.Value = FilaPoliza("MONTO_FRA")
                            CompNal.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "MXN" Then
                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_FRA")
                                CompNal.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_FRA")
                                CompNal.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(CompNal)
                        Else
                            If FilaPoliza("CREDIT1") = 0 Then
                                'se pone nodo OtrMetodoPago
                                Dim OtrMetodoPago As XmlNode = Doc.CreateElement("PLZ", "OtrMetodoPago", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                                Atributo = Doc.CreateAttribute("MetPagoPol")
                                Atributo.Value = "03"
                                OtrMetodoPago.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("Fecha")
                                Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                                OtrMetodoPago.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("Benef")
                                Atributo.Value = IIf(FilaPoliza("SUPNAME") = "", FilaPoliza("CUSTNAME"), FilaPoliza("SUPNAME"))
                                OtrMetodoPago.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("RFC")
                                Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                                OtrMetodoPago.Attributes.Append(Atributo)

                                If FilaPoliza("MONEDA_PLZA") <> "" Then
                                    Atributo = Doc.CreateAttribute("Monto")
                                    Atributo.Value = FilaPoliza("DEBIT1")
                                    OtrMetodoPago.Attributes.Append(Atributo)

                                    Atributo = Doc.CreateAttribute("Moneda")
                                    Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                    OtrMetodoPago.Attributes.Append(Atributo)

                                    Atributo = Doc.CreateAttribute("TipCamb")
                                    Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                    OtrMetodoPago.Attributes.Append(Atributo)
                                Else
                                    Atributo = Doc.CreateAttribute("Monto")
                                    Atributo.Value = FilaPoliza("DEBIT1")
                                    OtrMetodoPago.Attributes.Append(Atributo)
                                End If

                                Transaccion.AppendChild(OtrMetodoPago)
                            Else
                                'se pone nodo Transferenci
                                Dim Transferencia As XmlNode = Doc.CreateElement("PLZ", "Transferencia", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                                Atributo = Doc.CreateAttribute("CtaOri")
                                Atributo.Value = FilaPoliza("PAYACCOUNT")
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("BancoOriNal")
                                Atributo.Value = FilaPoliza("BANKCODE")
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("CtaDest")
                                Atributo.Value = IIf(FilaPoliza("PAYACCOUNT2") = "", "NO IDENTIFICADO", FilaPoliza("PAYACCOUNT2"))
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("BancoDestNal")
                                Atributo.Value = IIf(FilaPoliza("BANKCODE2") = "", "999", FilaPoliza("BANKCODE2"))
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("Fecha")
                                Atributo.Value = Format(CDate(FilaPoliza("PAYDATE")), "yyyy-MM-dd")
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("Benef")
                                Atributo.Value = IIf(FilaPoliza("SUPNAME") = "", FilaPoliza("CUSTNAME"), FilaPoliza("SUPNAME"))
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("RFC")
                                Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                                Transferencia.Attributes.Append(Atributo)

                                If FilaPoliza("MONEDA_PLZA") <> "" Then
                                    Atributo = Doc.CreateAttribute("Monto")
                                    Atributo.Value = FilaPoliza("CREDIT2")
                                    Transferencia.Attributes.Append(Atributo)

                                    Atributo = Doc.CreateAttribute("Moneda")
                                    Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                    Transferencia.Attributes.Append(Atributo)

                                    Atributo = Doc.CreateAttribute("TipCamb")
                                    Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                    Transferencia.Attributes.Append(Atributo)
                                Else
                                    Atributo = Doc.CreateAttribute("Monto")
                                    Atributo.Value = FilaPoliza("CREDIT1")
                                    Transferencia.Attributes.Append(Atributo)
                                End If

                                Transaccion.AppendChild(Transferencia)
                            End If
                        End If

                    Else
                        'se pone nodo CompExt
                        If FilaPoliza("BOOKNUM") <> "" Then
                            Dim CompExt As XmlNode = Doc.CreateElement("PLZ", "CompExt", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("NumFactExt")
                            Atributo.Value = FilaPoliza("BOOKNUM")
                            CompExt.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("TaxID")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            CompExt.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("MontoTotal")
                            Atributo.Value = FilaPoliza("MONTO_FRA")
                            CompExt.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "" Then
                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                CompExt.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                CompExt.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(CompExt)
                        Else
                            'se pone nodo Transferenci
                            Dim Transferencia As XmlNode = Doc.CreateElement("PLZ", "Transferencia", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("CtaOri")
                            Atributo.Value = FilaPoliza("PAYACCOUNT")
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("BancoOriNal")
                            Atributo.Value = FilaPoliza("BANKCODE")
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("CtaDest")
                            Atributo.Value = IIf(FilaPoliza("PAYACCOUNT2") = "", "NO IDENTIFICADO", FilaPoliza("PAYACCOUNT2"))
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("BancoDestNal")
                            Atributo.Value = "999"
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("BancoDestExt")
                            Atributo.Value = IIf(FilaPoliza("BANKCODE2") = "", "999", FilaPoliza("BANKCODE2"))
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Fecha")
                            Atributo.Value = Format(CDate(FilaPoliza("PAYDATE")), "yyyy-MM-dd")
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Benef")
                            Atributo.Value = IIf(FilaPoliza("SUPNAME") = "", FilaPoliza("CUSTNAME"), FilaPoliza("SUPNAME"))
                            Transferencia.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("RFC")
                            Atributo.Value = rdrEmisor("VATNUM")
                            Transferencia.Attributes.Append(Atributo)


                            If FilaPoliza("MONEDA_PLZA") <> "" Then
                                Atributo = Doc.CreateAttribute("Monto")
                                Atributo.Value = FilaPoliza("CREDIT2")
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                Transferencia.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                Transferencia.Attributes.Append(Atributo)
                            Else
                                Atributo = Doc.CreateAttribute("Monto")
                                Atributo.Value = FilaPoliza("CREDIT1")
                                Transferencia.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(Transferencia)
                        End If

                    End If

                    Poliza.AppendChild(Transaccion)
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)

            'Ciclo FOR para insertar las Pólizas de Recibos
            Poliza = Nothing
            Try
                Console.WriteLine("Recibos")
                Dim PolizaAnterior As String = ""
                For Each FilaPoliza As DataRow In dtPolizasRecibos.Rows
                    If FilaPoliza("FNCNUM") <> PolizaAnterior Then
                        If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)

                        'Console.WriteLine(FilaPoliza("FNCNUM"))
                        Poliza = Doc.CreateElement("PLZ", "Poliza", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("NumUnIdenPol")
                        Atributo.Value = FilaPoliza("FNCNUM")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Fecha")
                        Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Concepto")
                        Atributo.Value = FilaPoliza("DETAILS")
                        Poliza.Attributes.Append(Atributo)
                    End If

                    PolizaAnterior = FilaPoliza("FNCNUM")

                    Dim Transaccion As XmlNode = Doc.CreateElement("PLZ", "Transaccion", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = FilaPoliza("ACCNAME")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("DesCta")
                    Atributo.Value = FilaPoliza("ACCDES")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Concepto")
                    Atributo.Value = FilaPoliza("DETAILS")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Debe")
                    Atributo.Value = FilaPoliza("DEBIT1")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Haber")
                    Atributo.Value = FilaPoliza("CREDIT1")
                    Transaccion.Attributes.Append(Atributo)

                    'se pone nodo CompExt
                    If FilaPoliza("BOOKNUM") <> "" Then
                        Dim CompExt As XmlNode = Doc.CreateElement("PLZ", "CompExt", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("NumFactExt")
                        Atributo.Value = FilaPoliza("BOOKNUM")
                        CompExt.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("TaxID")
                        Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                        CompExt.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("MontoTotal")
                        Atributo.Value = FilaPoliza("MONTO_FRA")
                        CompExt.Attributes.Append(Atributo)

                        If FilaPoliza("MONEDA_PLZA") <> "" Then
                            Atributo = Doc.CreateAttribute("Moneda")
                            Atributo.Value = FilaPoliza("MONEDA_PLZA")
                            CompExt.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("TipCamb")
                            Atributo.Value = FilaPoliza("TCAMB_PLZA")
                            CompExt.Attributes.Append(Atributo)
                        End If

                        Transaccion.AppendChild(CompExt)
                    Else
                        If FilaPoliza("GLOB_FOLIOFISCAL") <> "" And FilaPoliza("GLOB_FOLIOFISCAL") <> "000000000000000000000000000000000000" Then
                            'se pone nodo CompNal
                            Dim CompNal As XmlNode = Doc.CreateElement("PLZ", "CompNal", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("UUID_CFDI")
                            Atributo.Value = FilaPoliza("GLOB_FOLIOFISCAL")
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("RFC")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("MontoTotal")
                            Atributo.Value = FilaPoliza("MONTO_FRA")
                            CompNal.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "MXN" Then
                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_FRA")
                                CompNal.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_FRA")
                                CompNal.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(CompNal)
                        Else
                            'Se pone nodo OtrMetodoPago
                            Dim OtrMetodoPago As XmlNode = Doc.CreateElement("PLZ", "OtrMetodoPago", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("MetPagoPol")
                            Atributo.Value = "03"
                            OtrMetodoPago.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Fecha")
                            Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                            OtrMetodoPago.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Benef")
                            Atributo.Value = IIf(FilaPoliza("SUPNAME") = "", FilaPoliza("CUSTNAME"), FilaPoliza("SUPNAME"))
                            OtrMetodoPago.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("RFC")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            OtrMetodoPago.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "" Then
                                Atributo = Doc.CreateAttribute("Monto")
                                Atributo.Value = IIf(FilaPoliza("DEBIT2") = 0, FilaPoliza("CREDIT2"), FilaPoliza("DEBIT2"))
                                OtrMetodoPago.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                OtrMetodoPago.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                OtrMetodoPago.Attributes.Append(Atributo)
                            Else
                                Atributo = Doc.CreateAttribute("Monto")
                                Atributo.Value = IIf(FilaPoliza("DEBIT1") = 0, FilaPoliza("CREDIT1"), FilaPoliza("DEBIT1"))
                                OtrMetodoPago.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(OtrMetodoPago)
                        End If
                    End If
                    Poliza.AppendChild(Transaccion)
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)


            'Ciclo FOR para insertar las Pólizas de Cheques
            Poliza = Nothing
            Try
                Console.WriteLine("Cheques")
                Dim PolizaAnterior As String = ""
                For Each FilaPoliza As DataRow In dtPolizasCheques.Rows
                    If FilaPoliza("FNCNUM") <> PolizaAnterior Then
                        If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)

                        'Console.WriteLine(FilaPoliza("FNCNUM"))
                        Poliza = Doc.CreateElement("PLZ", "Poliza", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("NumUnIdenPol")
                        Atributo.Value = FilaPoliza("FNCNUM")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Fecha")
                        Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Concepto")
                        Atributo.Value = FilaPoliza("DETAILS")
                        Poliza.Attributes.Append(Atributo)
                    End If

                    PolizaAnterior = FilaPoliza("FNCNUM")

                    Dim Transaccion As XmlNode = Doc.CreateElement("PLZ", "Transaccion", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = FilaPoliza("ACCNAME")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("DesCta")
                    Atributo.Value = FilaPoliza("ACCDES")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Concepto")
                    Atributo.Value = FilaPoliza("DETAILS")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Debe")
                    Atributo.Value = FilaPoliza("DEBIT1")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Haber")
                    Atributo.Value = FilaPoliza("CREDIT1")
                    Transaccion.Attributes.Append(Atributo)

                    If FilaPoliza("COUNTRYNAME").ToString.ToUpper = "MEXICO" Or FilaPoliza("COUNTRYNAME").ToString.ToUpper = "MÉXICO" Or FilaPoliza("COUNTRYNAME").ToString.ToUpper = "" Then
                        If FilaPoliza("GLOB_FOLIOFISCAL") <> "" And FilaPoliza("GLOB_FOLIOFISCAL") <> "000000000000000000000000000000000000" Then
                            'se pone nodo CompNal
                            Dim CompNal As XmlNode = Doc.CreateElement("PLZ", "CompNal", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("UUID_CFDI")
                            Atributo.Value = FilaPoliza("GLOB_FOLIOFISCAL")
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("RFC")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("MontoTotal")
                            Atributo.Value = FilaPoliza("MONTO_FRA")
                            CompNal.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "MXN" Then
                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_FRA")
                                CompNal.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_FRA")
                                CompNal.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(CompNal)
                        Else
                            'Se pone nodo Cheque
                            Dim Cheque As XmlNode = Doc.CreateElement("PLZ", "Cheque", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("Num")
                            Atributo.Value = FilaPoliza("BOOKNUM")
                            Cheque.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("BanEmisNal")
                            Atributo.Value = FilaPoliza("BANKCODE")
                            Cheque.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("CtaOri")
                            Atributo.Value = IIf(FilaPoliza("PAYACCOUNT") = "", "NO IDENTIFICADO", FilaPoliza("PAYACCOUNT"))
                            Cheque.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Fecha")
                            Atributo.Value = Format(CDate(FilaPoliza("PAYDATE")), "yyyy-MM-dd")
                            Cheque.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Benef")
                            Atributo.Value = IIf(FilaPoliza("SUPNAME") = "", FilaPoliza("CUSTNAME"), FilaPoliza("SUPNAME"))
                            Cheque.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("RFC")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            Cheque.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Monto")
                            Atributo.Value = FilaPoliza("MONTO_CHEQUE")
                            Cheque.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "" Then
                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                Cheque.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                Cheque.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(Cheque)
                        End If
                    Else
                        'se pone nodo CompExt
                        If FilaPoliza("BOOKNUM") <> "" Then
                            Dim CompExt As XmlNode = Doc.CreateElement("PLZ", "CompExt", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                            Atributo = Doc.CreateAttribute("NumFactExt")
                            Atributo.Value = FilaPoliza("BOOKNUM")
                            CompExt.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("TaxID")
                            Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                            CompExt.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("MontoTotal")
                            Atributo.Value = FilaPoliza("MONTO_FRA")
                            CompExt.Attributes.Append(Atributo)

                            If FilaPoliza("MONEDA_PLZA") <> "" Then
                                Atributo = Doc.CreateAttribute("Moneda")
                                Atributo.Value = FilaPoliza("MONEDA_PLZA")
                                CompExt.Attributes.Append(Atributo)

                                Atributo = Doc.CreateAttribute("TipCamb")
                                Atributo.Value = FilaPoliza("TCAMB_PLZA")
                                CompExt.Attributes.Append(Atributo)
                            End If

                            Transaccion.AppendChild(CompExt)

                        End If
                    End If
                    Poliza.AppendChild(Transaccion)
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)


            'Ciclo FOR para insertar las Pólizas de Otros
            Poliza = Nothing
            Try
                Console.WriteLine("Otros")
                Dim PolizaAnterior As String = ""
                For Each FilaPoliza As DataRow In dtPolizasOtros.Rows
                    If FilaPoliza("FNCNUM") <> PolizaAnterior Then
                        If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)

                        Poliza = Doc.CreateElement("PLZ", "Poliza", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("NumUnIdenPol")
                        Atributo.Value = FilaPoliza("FNCNUM")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Fecha")
                        Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                        Poliza.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Concepto")
                        Atributo.Value = FilaPoliza("DETAILS")
                        Poliza.Attributes.Append(Atributo)
                    End If

                    PolizaAnterior = FilaPoliza("FNCNUM")

                    Dim Transaccion As XmlNode = Doc.CreateElement("PLZ", "Transaccion", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = FilaPoliza("ACCNAME")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("DesCta")
                    Atributo.Value = FilaPoliza("ACCDES")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Concepto")
                    Atributo.Value = FilaPoliza("DETAILS")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Debe")
                    Atributo.Value = FilaPoliza("DEBIT1")
                    Transaccion.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Haber")
                    Atributo.Value = FilaPoliza("CREDIT1")
                    Transaccion.Attributes.Append(Atributo)

                    If FilaPoliza("GLOB_FOLIOFISCAL") <> "" And FilaPoliza("GLOB_FOLIOFISCAL") <> "000000000000000000000000000000000000" And FilaPoliza("GLOB_FOLIOFISCAL") <> "00000000-0000-0000-0000-000000000000" Then
                        'se pone nodo CompNal
                        Dim CompNal As XmlNode = Doc.CreateElement("PLZ", "CompNal", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("UUID_CFDI")
                        Atributo.Value = FilaPoliza("GLOB_FOLIOFISCAL")
                        CompNal.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("RFC")
                        Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                        CompNal.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("MontoTotal")
                        Atributo.Value = FilaPoliza("MONTO_FRA")
                        CompNal.Attributes.Append(Atributo)

                        If FilaPoliza("MONEDA_PLZA") <> "MXN" Then
                            Atributo = Doc.CreateAttribute("Moneda")
                            Atributo.Value = FilaPoliza("MONEDA_FRA")
                            CompNal.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("TipCamb")
                            Atributo.Value = FilaPoliza("TCAMB_FRA")
                            CompNal.Attributes.Append(Atributo)
                        End If

                        Transaccion.AppendChild(CompNal)
                    Else
                        'se pone nodo CompExt
                        'If FilaPoliza("BOOKNUM") <> "" Then
                        '    Dim CompExt As XmlNode = Doc.CreateElement("PLZ", "CompExt", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo")

                        '    Atributo = Doc.CreateAttribute("NumFactExt")
                        '    Atributo.Value = FilaPoliza("BOOKNUM")
                        '    CompExt.Attributes.Append(Atributo)

                        '    Atributo = Doc.CreateAttribute("TaxID")
                        '    Atributo.Value = IIf(FilaPoliza("RFCPROV") = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                        '    CompExt.Attributes.Append(Atributo)

                        '    Atributo = Doc.CreateAttribute("MontoTotal")
                        '    Atributo.Value = FilaPoliza("MONTO_FRA")
                        '    CompExt.Attributes.Append(Atributo)

                        '    If FilaPoliza("MONEDA_PLZA") <> "" Then
                        '        Atributo = Doc.CreateAttribute("Moneda")
                        '        Atributo.Value = FilaPoliza("MONEDA_PLZA")
                        '        CompExt.Attributes.Append(Atributo)

                        '        Atributo = Doc.CreateAttribute("TipCamb")
                        '        Atributo.Value = FilaPoliza("TCAMB_PLZA")
                        '        CompExt.Attributes.Append(Atributo)
                        '    End If

                        '    Transaccion.AppendChild(CompExt)
                        'Else
                        'Se pone nodo OtrMetodoPago
                        Dim OtrMetodoPago As XmlNode = Doc.CreateElement("PLZ", "OtrMetodoPago", "www.sat.gob.mx/esquemas/ContabilidadE/1_1/PolizasPeriodo")

                        Atributo = Doc.CreateAttribute("MetPagoPol")
                        Atributo.Value = "99"
                        OtrMetodoPago.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Fecha")
                        Atributo.Value = Format(CDate(FilaPoliza("BALDATE")), "yyyy-MM-dd")
                        OtrMetodoPago.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("Benef")
                        Atributo.Value = IIf(FilaPoliza("SUPNAME").ToString.Trim = "", FilaPoliza("CUSTNAME"), FilaPoliza("SUPNAME"))
                        OtrMetodoPago.Attributes.Append(Atributo)

                        Atributo = Doc.CreateAttribute("RFC")
                        If FilaPoliza("MONEDA_PLZA") = "MXN" Then
                            Atributo.Value = IIf(FilaPoliza("RFCPROV").ToString.Trim = "", FilaPoliza("RFCCTE"), FilaPoliza("RFCPROV"))
                        Else
                            Atributo.Value = "XEXX010101000"
                        End If
                        OtrMetodoPago.Attributes.Append(Atributo)

                        If FilaPoliza("MONEDA_PLZA") <> "" Then
                            Atributo = Doc.CreateAttribute("Monto")
                            Atributo.Value = IIf(FilaPoliza("DEBIT2") = 0, FilaPoliza("CREDIT2"), FilaPoliza("DEBIT2"))
                            OtrMetodoPago.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("Moneda")
                            Atributo.Value = FilaPoliza("MONEDA_PLZA")
                            OtrMetodoPago.Attributes.Append(Atributo)

                            Atributo = Doc.CreateAttribute("TipCamb")
                            Atributo.Value = FilaPoliza("TCAMB_PLZA")
                            OtrMetodoPago.Attributes.Append(Atributo)
                        Else
                            Atributo = Doc.CreateAttribute("Monto")
                            Atributo.Value = IIf(FilaPoliza("DEBIT1") = 0, FilaPoliza("CREDIT1"), FilaPoliza("DEBIT1"))
                            OtrMetodoPago.Attributes.Append(Atributo)
                        End If

                        Transaccion.AppendChild(OtrMetodoPago)
                        'End If
                    End If
                    Poliza.AppendChild(Transaccion)
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            If Not IsNothing(Poliza) Then Polizas.AppendChild(Poliza)


            If Not IsNothing(Polizas) Then Doc.AppendChild(Polizas)

            Dim Params As New FirmaParams
            Params.ArchivoXSD = "PolizasPeriodo_1_3.xsd"
            Params.ArchivoXSLT = "PolizasPeriodo_1_2.xslt"
            Params.NodoCert = "Polizas"
            Params.EspacioNombres = "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/PolizasPeriodo"
            Params.AtrCertif = "Certificado"
            Params.AtrNoCert = "noCertificado"
            Params.AtrSello = "Sello"

            Try
                Datos.Firma(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            Try
                Datos.Valida(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            If Not System.IO.Directory.Exists(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Polizas\") Then
                System.IO.Directory.CreateDirectory(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Polizas\")
            End If

            Dim RutaXML As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Polizas\" & rdrEmisor("VATNUM").ToString & Format(CInt(Anio), "0000") & Format(CInt(Periodo), "00") & "PL" & ".xml"
            Doc.Save(RutaXML)

            While 1
                Try
                    Dim xmlZIP As New ZipFile()
                    xmlZIP.AddFile(RutaXML, "")
                    xmlZIP.Save(RutaXML.Replace(".xml", ".zip"))
                    Console.WriteLine("")
                    Exit While
                Catch ex As Exception
                    Console.Write("*")
                    Exit Try
                End Try
            End While

            Console.WriteLine("¡Proceso concluido!")
            Console.Write("¿Desea abrir la carpeta? (S/N): ")

            If Console.ReadLine().ToUpper = "S" Then
                Shell("explorer.exe root = " & Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Polizas\", AppWinStyle.NormalFocus)
            End If
        Else
            Console.WriteLine("¡No se pudo generar el documento! Favor de verificar que tenga activada su cuenta." & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrCSD.Close()
        rdrEmisor.Close()
        'rdrBalanza.Close()
    End Sub

    Private Sub GeneraBalanzaSAT(ByVal Emp As String, ByVal Anio As String, ByVal Periodo As String, ByVal AnioId As String, ByVal FechaIni As String, ByVal FechaFin As String)
        Empresa = Emp

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT VATNUM " &
                                            "FROM " & Empresa & ".dbo.COMPDATA " &
                                            "WHERE COMP=-1")

        Dim FechaBalIni As Integer = Datos.GetDataScalar("SELECT SDATE-1440 FROM " & Empresa & ".dbo.GLPERIODS WHERE GL = " & AnioId & " AND PERIOD = " & Periodo & "")
        Dim FechaBalFin As Integer = Datos.GetDataScalar("SELECT EDATE FROM " & Empresa & ".dbo.GLPERIODS WHERE GL = " & AnioId & " AND PERIOD = " & Periodo & "")
        Dim query As String = "SELECT ACCOUNTS.ACCNAME, ACCOUNTS.GLOB_ACCNATURE, SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalIni & " THEN FNCBAL.BALANCE1*-1 ELSE 0.0 END) AS BALINICIAL, " &
                                    "SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.DEBIT1 ELSE 0.0 END) AS DEBITO, " &
                                    "SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.CREDIT1 ELSE 0.0 END) AS CREDITO, " &
                                    "SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.BALANCE1*-1 ELSE 0.0 END) AS BALFINAL " &
                                    "FROM " & Empresa & ".dbo.ACCOUNTS, " & Empresa & ".dbo.FNCBAL " &
                                    "WHERE ACCOUNTS.ACCOUNT <> 0 " &
                                    "AND ACCOUNTS.ACCOUNT   =  FNCBAL.ACCOUNT  " &
                                    "AND ACCOUNTS.TMPFLAG  <> 'Y' " &
                                    "AND FNCBAL.GL = " & AnioId & "  " &
                                    "AND (FNCBAL.CURDATE BETWEEN " & FechaBalIni & " " &
                                    "AND " & FechaBalFin & ") " &
                                    "AND ACCOUNTS.COMPANY  = -1 " &
                                    "GROUP BY ACCOUNTS.ACCNAME, ACCOUNTS.GLOB_ACCNATURE " &
                                    "HAVING SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.DEBIT1 ELSE 0.0 END)  <> 0.0 OR " &
                                    "SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & " THEN FNCBAL.CREDIT1 ELSE 0.0 END) <> 0.0 OR " &
                                    "SUM(CASE WHEN FNCBAL.CURDATE = " & FechaBalFin & "  THEN FNCBAL.BALANCE1*-1 ELSE 0.0 END) <> 0.0 " &
                                    "ORDER BY 1, 2 ASC"
        'Console.WriteLine(query)
        'Dim rdrBalanza As SqlDataReader = Datos.RegresaReader(query)
        Dim daBalanza As New SqlDataAdapter(query, Datos.ConnectionString)


        rdrCSD.Read()
        rdrEmisor.Read()
        'rdrBalanza.Read()

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine("Generando informe de Balanza para la empresa: " & rdrEmisor("VATNUM").ToString.Trim)

            Dim Fecha As Date = Format(Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now), "yyyy-MM-ddTHH:mm:ss")
            Doc = New XmlDocument()
            Dim Atributo As XmlAttribute
            Dim dtBalanza As New DataTable
            'dtBalanza.Load(rdrBalanza)
            daBalanza.Fill(dtBalanza)

            'Doc.Load(My.Application.Info.DirectoryPath & "\catBalanza-base.xml")
            Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?>" &
                        "<BCE:Balanza xsi:schemaLocation=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/BalanzaComprobacion http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/BalanzaComprobacion/BalanzaComprobacion_1_3.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:BCE=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/BalanzaComprobacion"" " &
                        "Version=""1.3"" RFC="""" Mes="""" Anio="""" TipoEnvio="""" > </BCE:Balanza>")
            Dim Balanza As XmlNode = Doc.GetElementsByTagName("Balanza", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/BalanzaComprobacion")(0)

            Balanza.Attributes("RFC").Value = rdrEmisor("VATNUM")
            Balanza.Attributes("Mes").Value = Periodo.PadLeft(2, "0")
            Balanza.Attributes("Anio").Value = CInt(Anio)

            If Anio > Fecha.Year Then
                Console.WriteLine("No se puede generar el documento porque el Año ingresado es mayor al año actual")
                Console.ReadLine()
                Exit Sub
            End If

            Dim TipoEnvio As String = ""

            Do
                Console.Write("Escriba ""N"" si es Normal, o ""C"" si es complementaria." & vbCrLf & "Tipo de envío: ")
                TipoEnvio = Console.ReadLine.Trim.ToUpper
            Loop While (TipoEnvio <> "N" And TipoEnvio <> "C")

            Balanza.Attributes("TipoEnvio").Value = TipoEnvio

            If TipoEnvio = "C" Then
                Atributo = Doc.CreateAttribute("FechaModBal")
                Atributo.Value = Format(Fecha, "yyyy-MM-dd")
                Balanza.Attributes.Append(Atributo)
            End If

            Dim firstFlag As Boolean = True
            Dim Ctas As XmlNode = Nothing
            Try
                For Each Cuenta As DataRow In dtBalanza.Rows
                    'If Cuenta("BALINICIAL") <> 0 Or Cuenta("DEBITO") <> 0 Or Cuenta("CREDITO") <> 0 Then
                    'Console.WriteLine(Cuenta("ACCNAME") & "|" & Cuenta("BALINICIAL") & "|" & Cuenta("DEBITO") & "|" & Cuenta("CREDITO") & "|" & Cuenta("BALFINAL"))
                    'If firstFlag Then
                    Ctas = Doc.CreateElement("BCE", "Ctas", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/BalanzaComprobacion")

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = Cuenta("ACCNAME")
                    Ctas.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("SaldoIni")
                    Atributo.Value = IIf(Cuenta("GLOB_ACCNATURE") = "A", -1 * Cuenta("BALINICIAL"), Cuenta("BALINICIAL")) 'IIf(Cuenta("ACCNAME").ToString.StartsWith("2") Or Cuenta("ACCNAME").ToString.StartsWith("3") Or Cuenta("ACCNAME").ToString.StartsWith("4"), -1 * Cuenta("BALINICIAL"), Cuenta("BALINICIAL")) 'Cuenta("BALINICIAL")
                    Ctas.Attributes.Append(Atributo)

                    'firstFlag = False
                    'Else
                    Atributo = Doc.CreateAttribute("Debe")
                    Atributo.Value = Cuenta("DEBITO")
                    Ctas.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Haber")
                    Atributo.Value = Cuenta("CREDITO")
                    Ctas.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("SaldoFin")
                    Atributo.Value = IIf(Cuenta("GLOB_ACCNATURE") = "A", -1 * Cuenta("BALFINAL"), Cuenta("BALFINAL")) 'IIf(Cuenta("ACCNAME").ToString.StartsWith("2") Or Cuenta("ACCNAME").ToString.StartsWith("3") Or Cuenta("ACCNAME").ToString.StartsWith("4"), -1 * Cuenta("BALFINAL"), Cuenta("BALFINAL")) 'Cuenta("BALFINAL")
                    Ctas.Attributes.Append(Atributo)

                    Balanza.AppendChild(Ctas)

                    'firstFlag = True
                    'End If
                    'End If
                Next
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Doc.AppendChild(Balanza)

            Dim Params As New FirmaParams
            Params.ArchivoXSD = "BalanzaComprobacion_1_3.xsd"
            Params.ArchivoXSLT = "BalanzaComprobacion_1_2.xslt"
            Params.NodoCert = "Balanza"
            Params.EspacioNombres = "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/BalanzaComprobacion"
            Params.AtrCertif = "Certificado"
            Params.AtrNoCert = "noCertificado"
            Params.AtrSello = "Sello"

            Try
                Datos.Firma(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            Try
                Datos.Valida(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            If Not System.IO.Directory.Exists(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Balanza\") Then
                System.IO.Directory.CreateDirectory(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Balanza\")
            End If

            Dim RutaXML As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Balanza\" & rdrEmisor("VATNUM").ToString & Format(CInt(Anio), "0000") & Format(CInt(Periodo), "00") & "B" & TipoEnvio & ".xml"
            Doc.Save(RutaXML)

            While 1
                Try
                    Dim xmlZIP As New ZipFile()
                    xmlZIP.AddFile(RutaXML, "")
                    xmlZIP.Save(RutaXML.Replace(".xml", ".zip"))
                    Console.WriteLine("")
                    Exit While
                Catch ex As Exception
                    Console.Write("*")
                    Exit Try
                End Try
            End While

            Console.WriteLine("¡Proceso concluido!")
            Console.Write("¿Desea abrir la carpeta? (S/N): ")

            If Console.ReadLine().ToUpper = "S" Then
                Shell("explorer.exe root = " & Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\Balanza\", AppWinStyle.NormalFocus)
            End If
        Else
            Console.WriteLine("¡No se pudo generar el documento! Favor de verificar que tenga activada su cuenta." & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrCSD.Close()
        rdrEmisor.Close()
        'rdrBalanza.Close()
    End Sub

    Private Sub GeneraCatCtasSAT(ByVal Emp As String, ByVal Periodo As String, ByVal Anio As String)
        Dim Empresa As String = Emp

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT VATNUM " &
                                            "FROM " & Empresa & ".dbo.COMPDATA " &
                                            "WHERE COMP=-1")

        Dim query As String = "SELECT A1.BALTYPE, G1.SATACCNAME, G1.SATACCDES, G2.SATSUBACCNAME, G2.SATSUBACCDES, A.ACCNAME, A.ACCDES, A.GLOB_ACCNATURE  " &
                                                                "FROM " & Empresa & ".dbo.ACCOUNTS A, " & Empresa & ".dbo.GLOB_SATACCOUNTS G1, " & Empresa & ".dbo.GLOB_SATSUBACCOUNTS G2, " & Empresa & ".dbo.SECTIONS S, " & Empresa & ".dbo.ACCTYPES A1 " &
                                                                "WHERE(A.GLOB_SATACCOUNT = G1.SATACCOUNT And A.GLOB_SATSUBACCOUNT = G2.SATSUBACCOUNT And A.GLOB_SATACCOUNT <> 0) " &
                                                                "AND A.SECTION = S.SECTION AND S.ACCTYPE = A1.ACCTYPE " &
                                                                "GROUP BY A1.BALTYPE, G1.SATACCNAME, G1.SATACCDES, G2.SATSUBACCNAME, G2.SATSUBACCDES, A.ACCNAME, A.ACCDES, A.GLOB_ACCNATURE   " &
                                                                "ORDER BY 1 DESC, 2, 4, 6"

        'Console.WriteLine(query)
        Dim daCatCtas As New SqlDataAdapter(query, Datos.ConnectionString)
        'Dim rdrCatCtas As SqlDataReader = Datos.RegresaReader(query)

        rdrCSD.Read()
        rdrEmisor.Read()
        'rdrCatCtas.Read()

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine("Generando Catálogo de Cuentas para la empresa: " & rdrEmisor("VATNUM").ToString.Trim)

            Dim Fecha As Date = Format(Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now), "yyyy-MM-ddTHH:mm:ss")
            'Dim Doc As New XmlDocument()
            Doc = New XmlDocument()
            Dim Atributo As XmlAttribute
            'Dim Catalogo As XmlNode
            Dim dtCuentas As New DataTable
            daCatCtas.Fill(dtCuentas)
            'dtCuentas.Load(rdrCatCtas)

            'Doc.Load(My.Application.Info.DirectoryPath & "\catCtas-base.xml")
            Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?>" &
                        "<catalogocuentas:Catalogo xsi:schemaLocation=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas/CatalogoCuentas_1_3.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:catalogocuentas=""http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas"" " &
                        "Version=""1.3"" RFC="""" Mes="""" Anio="""" ></catalogocuentas:Catalogo>")
            Dim Catalogo As XmlNode = Doc.GetElementsByTagName("Catalogo", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas")(0)

            'Atributo = Doc.CreateAttribute("Version")
            'Atributo.Value = "1.1"
            'Catalogo.Attributes.Append(Atributo)

            Catalogo.Attributes("RFC").Value = rdrEmisor("VATNUM")
            Catalogo.Attributes("Mes").Value = Periodo.PadLeft(2, "0") 'Format(Fecha.Month, "00")
            Catalogo.Attributes("Anio").Value = Anio 'Format(Fecha.Year, "00")

            Dim index1 As String = "0"
            Dim index2 As String = "0"
            Dim index3 As Integer = 1
            For Each Cuenta As DataRow In dtCuentas.Rows
                If index1 <> Cuenta("SATACCNAME") Then
                    Dim Ctas1 As XmlNode = Doc.CreateElement("catalogocuentas", "Ctas", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas")
                    index1 = Cuenta("SATACCNAME")
                    index2 = 0

                    Atributo = Doc.CreateAttribute("CodAgrup")
                    Atributo.Value = Cuenta("SATACCNAME")
                    Ctas1.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = Cuenta("SATACCNAME")
                    Ctas1.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Desc")
                    Atributo.Value = Cuenta("SATACCDES")
                    Ctas1.Attributes.Append(Atributo)

                    'Atributo = Doc.CreateAttribute("SubCtaDe")
                    'Atributo.Value = Format(Fecha.Month, "00")
                    'Ctas.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Nivel")
                    Atributo.Value = 1
                    Ctas1.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Natur")
                    'Atributo.Value = IIf(Cuenta("BALTYPE") = -1, "D", "A")
                    Atributo.Value = Cuenta("GLOB_ACCNATURE") 'IIf(Cuenta("SATACCNAME").ToString.StartsWith("2") Or Cuenta("SATACCNAME").ToString.StartsWith("3") Or Cuenta("SATACCNAME").ToString.StartsWith("4"), "A", "D")
                    Ctas1.Attributes.Append(Atributo)

                    Catalogo.AppendChild(Ctas1)
                End If

                If index2 <> Cuenta("SATSUBACCNAME") Then
                    Dim Ctas2 As XmlNode = Doc.CreateElement("catalogocuentas", "Ctas", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas")
                    index2 = Cuenta("SATSUBACCNAME")
                    index3 = 1

                    Atributo = Doc.CreateAttribute("CodAgrup")
                    Atributo.Value = Cuenta("SATSUBACCNAME")
                    Ctas2.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("NumCta")
                    Atributo.Value = Cuenta("SATSUBACCNAME")
                    Ctas2.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Desc")
                    Atributo.Value = Cuenta("SATSUBACCDES")
                    Ctas2.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("SubCtaDe")
                    Atributo.Value = Cuenta("SATACCNAME")
                    Ctas2.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Nivel")
                    Atributo.Value = 2
                    Ctas2.Attributes.Append(Atributo)

                    Atributo = Doc.CreateAttribute("Natur")
                    'Atributo.Value = IIf(Cuenta("BALTYPE") = -1, "D", "A")
                    Atributo.Value = Cuenta("GLOB_ACCNATURE") 'IIf(Cuenta("SATACCNAME").ToString.StartsWith("2") Or Cuenta("SATACCNAME").ToString.StartsWith("3") Or Cuenta("SATACCNAME").ToString.StartsWith("4"), "A", "D")
                    Ctas2.Attributes.Append(Atributo)

                    Catalogo.AppendChild(Ctas2)
                End If

                Dim Ctas3 As XmlNode = Doc.CreateElement("catalogocuentas", "Ctas", "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas")

                Atributo = Doc.CreateAttribute("CodAgrup")
                Atributo.Value = Cuenta("SATSUBACCNAME") ' & "." & index3.ToString
                Ctas3.Attributes.Append(Atributo)

                Atributo = Doc.CreateAttribute("NumCta")
                Atributo.Value = Cuenta("ACCNAME")
                Ctas3.Attributes.Append(Atributo)

                Atributo = Doc.CreateAttribute("Desc")
                Atributo.Value = Cuenta("ACCDES")
                Ctas3.Attributes.Append(Atributo)

                Atributo = Doc.CreateAttribute("SubCtaDe")
                Atributo.Value = Cuenta("SATSUBACCNAME")
                Ctas3.Attributes.Append(Atributo)

                Atributo = Doc.CreateAttribute("Nivel")
                Atributo.Value = 3
                Ctas3.Attributes.Append(Atributo)

                Atributo = Doc.CreateAttribute("Natur")
                'Atributo.Value = IIf(Cuenta("BALTYPE") = -1, "D", "A")
                Atributo.Value = Cuenta("GLOB_ACCNATURE") 'IIf(Cuenta("SATACCNAME").ToString.StartsWith("2") Or Cuenta("SATACCNAME").ToString.StartsWith("3") Or Cuenta("SATACCNAME").ToString.StartsWith("4"), "A", "D")
                Ctas3.Attributes.Append(Atributo)

                Catalogo.AppendChild(Ctas3)
                index3 += 1
            Next

            Doc.AppendChild(Catalogo)

            Dim Params As New FirmaParams
            Params.ArchivoXSD = "CatalogoCuentas_1_3.xsd"
            Params.ArchivoXSLT = "CatalogoCuentas_1_2.xslt"
            Params.NodoCert = "Catalogo"
            Params.EspacioNombres = "http://www.sat.gob.mx/esquemas/ContabilidadE/1_3/CatalogoCuentas"
            Params.AtrCertif = "Certificado"
            Params.AtrNoCert = "noCertificado"
            Params.AtrSello = "Sello"

            Try
                Datos.Firma(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            Try
                Datos.Valida(Params)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.ReadLine()
                Exit Sub
            End Try

            If Not System.IO.Directory.Exists(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\CatalogoCtas\") Then
                System.IO.Directory.CreateDirectory(Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\CatalogoCtas\")
            End If

            'Dim RutaXML As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\CatalogoCtas\" & Format(Fecha, "yyyy-MM-dd_HHmmss") & ".xml"
            Dim RutaXML As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\CatalogoCtas\" & rdrEmisor("VATNUM").ToString & Anio & Periodo.PadLeft(2, "0") & "CT.xml"
            Doc.Save(RutaXML)

            While 1
                Try
                    Dim xmlZIP As New ZipFile()
                    xmlZIP.AddFile(RutaXML, "")
                    xmlZIP.Save(RutaXML.Replace(".xml", ".zip"))
                    Console.WriteLine("")
                    Exit While
                Catch ex As Exception
                    Console.Write("*")
                    Exit Try
                End Try
            End While


            Console.WriteLine("¡Proceso concluido!")
            Console.Write("¿Desea abrir la carpeta? (S/N): ")

            If Console.ReadLine().ToUpper = "S" Then
                Shell("explorer.exe root = " & Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM").ToString & "\CatalogoCtas\", AppWinStyle.NormalFocus)
            End If
        Else
            Console.WriteLine("¡No se pudo generar el documento! Favor de verificar que tenga activada su cuenta." & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrCSD.Close()
        rdrEmisor.Close()
        'rdrCatCtas.Close()
    End Sub

    Private Sub StatusFolios(ByVal RFC As String)
        Dim ds As New DataSet
        Try
            ds = Datos.ObtieneStatusFolios(RFC)
        Catch ex As Exception
            Console.WriteLine(ex.Message & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
            Exit Sub
        End Try

        Console.WriteLine("------------------------------------------------------------------------------")
        Console.WriteLine("| Status               | Solicitados | Restantes   | Desde      | Hasta      |")
        Console.WriteLine("------------------------------------------------------------------------------")
        For Each fila As DataRow In ds.Tables(0).Rows
            Console.WriteLine(String.Format("| {0,-20} | {1,11} | {2,11} | {3,-10:d} | {4,-10:d} |", fila("Status").ToString, fila("Solicitados").ToString, fila("Restantes").ToString, CDate(fila("Desde").ToString), CDate(fila("Hasta").ToString)))
        Next

        Console.WriteLine("------------------------------------------------------------------------------")
        Console.WriteLine(vbCrLf & "Presione cualquier tecla para continuar...")
        Console.Read()
    End Sub

    Private Function ValidaLicencia() As Boolean
        Dim NoUsuarios As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoUsuarios", String.Empty))
        Dim NoSerie As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoSerie", String.Empty))
        Dim NoLicencia As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoLicencia", String.Empty))
        Dim NoHDKey As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoHD", String.Empty))
        Dim HDSerial As String = ""

        Dim fso As New FileSystemObject
        Dim HD As Drive = fso.GetDrive("C:\")
        If HD.IsReady Then
            HDSerial = HD.SerialNumber.ToString
        Else
            Console.WriteLine("¡El dispositivo no está listo!" & vbCrLf & "Sin esto no podrá autorizar la licencia del producto.")
            Console.Read()
            Return False
        End If

        Try
            Dim wsLicencia As New LicenciasSAF.Service1
            Dim ds As DataSet = wsLicencia.PermLicencia(NoSerie, HDSerial)
            Dim fila As DataRow = ds.Tables(0).Rows(0)
            If Trim(fila("Mensaje")) = "AUTORIZADO" Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub VerCFDI(ByVal IV As String, ByVal Emp As String)
        Empresa = Emp
        Dim rdrPDF As SqlDataReader = Datos.RegresaReader("SELECT PDF, XML FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Emp & "'")
        rdrPDF.Read()

        If rdrPDF.HasRows Then
            'If System.IO.File.Exists(Datos.RutaDocs & "\temp.pdf") Then System.IO.File.Delete(Datos.RutaDocs & "\temp.pdf")

            If rdrPDF("PDF") Is System.DBNull.Value Then
                Console.WriteLine("Generando Cadena Original")
                Doc = New XmlDocument
                Doc.Load(rdrPDF.GetSqlXml(1).CreateReader())
                Doc.InsertBefore(Doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty), Doc.DocumentElement)
                ' Convierte a memorystream
                Dim msXML_TFD As MemoryStream = New MemoryStream
                Dim writer_TFD As XmlTextWriter = New XmlTextWriter(msXML_TFD, UTF8withoutBOM)
                Dim timbre As XmlNode = Doc.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0)
                Dim docTimbre As New XmlDocument()
                docTimbre.CreateXmlDeclaration("1.0", "UTF-8", "")
                docTimbre.AppendChild(docTimbre.ImportNode(timbre, True))
                docTimbre.Save(writer_TFD)

                ' Genera la cadena original
                For i = 1 To 5
                    Console.WriteLine("Intento " & i.ToString & " de 5.")
                    Dim msChain As MemoryStream = New MemoryStream()
                    Try
                        Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
                        Dim xslt As XslCompiledTransform = New XslCompiledTransform()
                        xslt.Load("cadenaoriginal_TFD_1_1.xslt")
                        msXML_TFD.Position = 0
                        Dim xp As XPathDocument = New XPathDocument(msXML_TFD)
                        xslt.Transform(xp, tw)
                        CadenaOriginal_TFD = Encoding.ASCII.GetString(msChain.ToArray).Normalize
                        Console.WriteLine("OK - Cadena original del TFD generada con éxito.")
                        Exit For
                    Catch ex As Exception
                        If i = 5 Then
                            Console.WriteLine("Error - No se pudo generar la cadena original del TFD.")
                        End If
                        Exit Try
                    End Try
                Next

                Console.WriteLine("Generando PDF")
                rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

                rdrEmisor = Datos.RegresaReader("SELECT (A.COMPDES+ ' ' + A.GLFO_COMPNOM) AS COMPDES, A.ADDRESS, A.PHONE, A.FAX, A.VATNUM, A.GLFO_REGIMEN, A.EMAIL, A.WEBSITE, A.GLFO_NUMERO, A.GLFO_MUNICIPIO, A.GLFO_COLONIA,A.GLFO_DELEG,A.STATE AS STATENAME, E.COUNTRYNAME, B.EXTFILENAME, A.ZIP " &
                                                                        "FROM " & Empresa & ".dbo.COMPDATA AS A, " & Empresa & ".dbo.COUNTRIES AS E, " & Empresa & ".dbo.COMPDATAA AS B " &
                                                                        "WHERE A.COMP=-1 AND A.COUNTRY=E.COUNTRY AND A.COMP=B.COMP")

                rdrReceptor = Datos.RegresaReader("SELECT A.IV, A.IVNUM,B.VATNUM, (B.CUSTDES + ' ' + B.GLFO_COMPNOM) AS CUSTDES, B.CUST, B.ADDRESS, B.GLFO_NUMERO, B.GLFO_MUNICIPIO,B.GLFO_COLONIA,B.GLFO_DELEG, B.GLFO_NOCUENTA, B.GLFO_CONDPAGO, B.GLOB_EMAIL, B.GLFO_NUMPRO, B.GLFO_TIPOP, B.GLFO_GLN1, B.GLFO_GLN2, B.GLOB_ADENDAAMC,C.STATENAME, D.COUNTRYNAME, D.COUNTRYCODE, C2.EMAIL, B.ZIP, P.PAYDES " &
                                                                        "FROM " & Empresa & ".dbo.INVOICES AS A, " & Empresa & ".dbo.CUSTOMERS AS B, " & Empresa & ".dbo.STATES AS C, " & Empresa & ".dbo.COUNTRIES AS D, " & Empresa & ".dbo.CUSTOMERSA C2, " & Empresa & ".dbo.PAY P " &
                                                                        "WHERE A.CUST = B.CUST AND A.IV=" & IV & " AND B.STATEID=C.STATEID AND B.COUNTRY = D.COUNTRY AND C2.CUST = B.CUST AND B.PAY=P.PAY ")

                rdrFactGral = Datos.RegresaReader("SELECT I.IV,I.IVNUM, I.ORD, I.GLFO_FACTORAJE,CL.Nombre, CL.RFC, I.GLFO_CLIENTEFACT, CAST(DATEADD(DAY, I.GLFO_FECPAGO/1440, '1988-01-01') AS DATE) AS GLFO_FECPAGO, I.GLFO_OTRASFE, I.DOC, CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE) AS FECHA_CREACION, " &
                                                                "I.QPRICE, I.DISCOUNT, I.DISPRICE, I.VAT, I.TOTPRICE, I.VATPRICE, I.WTAX, I.AFTERWTAX, I.GLFO_USOCFDI, I.GLFO_METODOP, I.GLOB_FORMAPAGO, I.GLFO_SUSTITUYE, I.GLFO_IVCANCELA, I.GLFO_TRELACION, CU.CODE, CU.NAME, CU.EXCHANGE, I.DEBIT, I.IVREF, I.FINAL, I.GLFO_ADUANA, I.GLFO_FOLGR, CAST(DATEADD(DAY, I.GLFO_FECGR/1440, '1988-01-01') AS DATE) AS GLFO_FECGR, I.GLFO_TIENDAE, O.REFERENCE, CAST(DATEADD(DAY, O.GLFO_REFDATE/1440, '1988-01-01') AS DATE) AS GLFO_REFDATE, " &
                                                                "CAST(DATEADD(DAY, I.GLFO_FECPED/1440, '1988-01-01') AS DATE) AS GLFO_FECPED, I.GLFO_NPEDI, P.PAYDES, P.PAYCODE, " &
                                                                "I.FNCTRANS, I.TYPE, CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE) AS MARCA_D_TIEMPO, I.GLBF_ORDENCOMPRA, CAST(DATEADD(DAY, I.GLBF_FECH_OC/1440, '1988-01-01') AS DATE) AS GLBF_FECH_OC, " &
                                                                "I.WTAX, I.WTAXPERCENT, I.STORNOFLAG, T.TAXDES, T.TAXPERCENT, BR.BRANCHDES, (BR.ADDRESS+ ' ' + BR.GLFO_COMPNOM) AS ADDRESS, " &
                                                                "BR.STATE, CR.COUNTRYNAME, BR.ZIP, BR.PHONE, I.DETAILS, " &
                                                                "I.T$PERCENT, CAST(DATEADD(DAY, I.BALDATE/1440, '1988-01-01') AS DATE) AS FECHA_TRANSACCION, " &
                                                                "(SELECT GLFO.OBS1+'|'+GLFO.OBS2+'|'+GLFO.OBS3+'|'+GLFO.OBS4+'|'+GLFO.OBS5 FROM " & Empresa & ".dbo.GLFO_OBSFACTURAS GLFO, " & Empresa & ".dbo.INVOICES I WHERE GLFO.IV = I.IV AND I.IV = " & IV & ") AS OBSERVACIONES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES I LEFT JOIN " & Empresa & ".dbo.GLFO_CLIENTEFACT CL ON I.GLFO_CLIENTEFACT = CL.RFC, " & Empresa & ".dbo.PAY P, " & Empresa & ".dbo.TAXES T, " & Empresa & ".dbo.BRANCHES BR, " & Empresa & ".dbo.COUNTRIES CR, " &
                                                                "" & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.ORDERS O WHERE I.IV =" & IV & " AND I.FINAL = 'Y' AND I.PAY=P.PAY " &
                                                                "and I.TAX=T.TAX AND I.BRANCH=BR.BRANCH AND BR.COUNTRY=CR.COUNTRY AND CU.CURRENCY=I.CURRENCY AND I.ORD=O.ORD")

                Dim daItems As New SqlDataAdapter("SELECT I.IV,I.PART,IX.IVTAX,P.PARTNAME, (P.PARTDES + ' ' + I.GLB_DESC_1 + ' ' + I.GLB_DESC_2) AS PARTDES, P.GLFO_CLAVEP, I.PRICE,I.TQUANT/1000.00 AS QUANT,CU.CODE,I.LINE, " &
                                                                        "CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE)AS FECHA_CREACION, I.T$PERCENT,I.QPRICE,I.TUNIT,U.UNITNAME,U.UNITDES, TX.TAXCODE, TX.TAXPERCENT, " &
                                                                        "CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE)AS MARCA_D_TIEMPO,(select  TOP 1 SERIALNAME FROM " & Empresa & ".dbo.SERIAL S WHERE P.PART=S.PART) as PEDIMENTO, " &
                                                                        "(SELECT  TOP 1 S.ATCUST FROM " & Empresa & ".dbo.SERIAL S WHERE S.PART=P.PART) AS ATCUST, (SELECT TEXT FROM  " & Empresa & ".dbo.NONSTANDARD WHERE NONSTANDARD.NONSTANDARD = I.NONSTANDARD AND I.PART = P.PART) AS NONSTANDARD, ((SELECT TEXT FROM  " & Empresa & ".dbo.NONSTANDARD WHERE NONSTANDARD.NONSTANDARD = I.NONSTANDARD AND I.PART = P.PART) + ' ' + I.GLB_DESC_1 + ' ' + I.GLB_DESC_2) AS PARTDES2, " &
                                                                        "(SELECT node.text FROM (SELECT DISTINCT(IV) FROM " & Empresa & ".dbo.INVOICEITEMSTEXT WHERE IV=" & IV & " AND I.KLINE = KLINE) AS IT CROSS APPLY(SELECT (TEXT + ' ') AS '*' FROM " & Empresa & ".dbo.INVOICEITEMSTEXT WHERE IV=" & IV & " AND I.KLINE = KLINE FOR XML PATH('')) AS node(text))  AS OBSERVACIONES " &
                                                                        "FROM " & Empresa & ".dbo.INVOICEITEMS I, " & Empresa & ".dbo.INVOICEITEMSA IX, " & Empresa & ".dbo.PART P, " & Empresa & ".dbo.UNIT U, " & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.TAXES TX " &
                                                                        "WHERE I.PART = P.PART AND I.IV = IX.IV AND I.KLINE=IX.KLINE AND P.PUNIT = U.UNIT AND I.IV=" & IV & " AND I.CURRENCY=CU.CURRENCY AND TX.TAX = IX.WTAXTBL", Datos.ConnectionString)

                Dim facturasPagadas As New SqlDataAdapter("SELECT  F.CREDIT1, F.CREDIT2, FAUX.GLFO_RELAC, X.IVNUM, I.IVNUM AS IVNUMFACT, X.TOTPRICE, FA.FNCIREF1, I.GLOB_FOLIOFISCAL, I.GLFO_METODOP, FB.IVBALANCE, FBA.IVBALANCE2, I.TOTPRICE AS TOTALFACTURA, CU.CODE, CU.EXCHANGE, (SELECT COUNT(1) FROM " & Empresa & ".dbo.FNCITEMSA FAX, " & Empresa & ".dbo.FNCITEMS F, " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.FNCTRANS FT " &
                                                  "WHERE FAX.FNCIREF1 = FA.FNCIREF1 AND FAX.FNCTRANS=F.FNCTRANS AND I.FNCTRANS=FAX.FNCTRANS AND F.LINE=FAX.KLINE AND FT.FNCTRANS=F.FNCTRANS AND I.TYPE = 'T' AND FT.CFNCTRANS = 0 AND F.QIV <> 0) AS  PARCIALIDAD " &
                                                  "FROM " & Empresa & ".dbo.INVOICES X, " & Empresa & ".dbo.FNCITEMS F, " & Empresa & ".dbo.FNCITEMSA FA, " & Empresa & ".dbo.FNCITEMSB FBA, " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.FNCITEMS FAUX, " & Empresa & ".dbo.FNCITEMSA FB, " & Empresa & ".dbo.CURRENCIES CU " &
                                                  "WHERE X.IV =" & IV & " AND X.FNCTRANS=F.FNCTRANS AND X.ACCOUNT = F.ACCOUNT AND FBA.FNCTRANS=FB.FNCTRANS AND FBA.KLINE=1 AND FA.FNCTRANS=X.FNCTRANS AND F.KLINE=FA.KLINE AND I.IVNUM = FA.FNCIREF1 AND FAUX.FNCTRANS = I.FNCTRANS AND FB.KLINE = FAUX.KLINE AND FB.FNCTRANS=I.FNCTRANS AND I.CURRENCY=CU.CURRENCY AND I.GLOB_FOLIOFISCAL <> '' AND FB.IVBALANCE <> 0", Datos.ConnectionString)

                rdrBanco = Datos.RegresaReader("SELECT C.PAYACCOUNT, C.BIC, C.CASHNAME FROM " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.CASH C WHERE I.IV =" & IV & " AND C.CASH = I.TOCASH")

                rdrFactPagadas = New DataTable
                facturasPagadas.Fill(rdrFactPagadas)
                rdrBanco.Read()
                rdrFactItems = New DataTable
                daItems.Fill(rdrFactItems)
                rdrEmisor.Read()
                rdrReceptor.Read()
                rdrFactGral.Read()
                rdrCSD.Read()
                If rdrFactGral("GLFO_SUSTITUYE") = "Y" Then
                    rdrFactRel = Datos.RegresaReader("SELECT GLOB_FOLIOFISCAL FROM " & Empresa & ".dbo.INVOICES WHERE IVNUM='" & rdrFactGral("GLFO_IVCANCELA") & "'")
                    rdrFactRel.Read()
                End If
                For Each Item As DataRow In rdrFactItems.Rows
                    If Item("IVTAX") > 0 Then
                        Tras = True
                        Exit For
                    End If

                Next
                If rdrFactGral("TYPE") = "T" Then reciboPago = True
                Console.WriteLine(reciboPago)
                If reciboPago Then modificaXMLRecibo(Doc)
                For Each Item As DataRow In rdrFactItems.Rows
                    If Not String.IsNullOrEmpty(Item("TAXPERCENT")) And Item("TAXPERCENT") > 0 Then
                        Retencion = True
                        Exit For
                    End If
                Next
                Dim msXML As MemoryStream = New MemoryStream
                Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
                Doc.Save(writer)
                msXML.Position = 0
                Dim dsNuevoComp As New System.Data.DataSet
                dsNuevoComp.ReadXml(msXML)
                Datos.CreaDataSetCFDI3_3(dsNuevoComp, False, CadenaOriginal_TFD, IIf(rdrFactGral("DEBIT").ToString.Trim = "D", "FACTURA", "NOTA DE CREDITO"), True)

                Dim rutaPDF As String = NombreAleatorio(".pdf")
                GeneraReporte(dsNuevoComp, rutaPDF)
                Shell("explorer.exe " & rutaPDF)
            Else
                Try
                    Dim rutaPDF As String = NombreAleatorio(".pdf")
                    System.IO.File.WriteAllBytes(rutaPDF, rdrPDF("PDF"))
                    Shell("explorer.exe " & rutaPDF)
                Catch ex As Exception
                    Console.WriteLine("No se encontró el documento!" & vbCrLf & "Presione cualquier tecla para continuar...")
                    Console.Read()
                End Try
            End If
        Else
            Console.WriteLine("No se encontró el documento!" & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrPDF.Close()
        rdrReceptor.Close()
        rdrEmisor.Close()
        rdrFactGral.Close()
        rdrCSD.Close()
    End Sub

    Private Function NombreAleatorio(ByVal extension As String) As String
        Dim fileflag As Boolean = False
        Dim ruta As String = Datos.RutaDocs & "\0000" & extension
        If Not extension.StartsWith(".") Then
            extension = "." & extension
        End If

        While Not fileflag
            If System.IO.File.Exists(ruta) Then
                Try
                    System.IO.File.Delete(ruta)
                    fileflag = True
                Catch ex As Exception
                    Dim nombre As String = CInt(Rnd() * 1000).ToString
                    ruta = Datos.RutaDocs & "\" & nombre.PadLeft(3, "0") & extension
                End Try
            Else
                fileflag = True
            End If
        End While

        Return ruta
    End Function

    Private Sub MenuCompania()
        Dim cia As String = ""
        Console.Clear()
        Console.WriteLine("*************** MENU COMPAÑIA *****************" & vbCrLf)
        Console.WriteLine("1 - Seleccionar Compañía")
        Console.WriteLine("2 - Agregar Compañía")
        Console.WriteLine("3 - Establecer Ruta del CSD")
        Console.WriteLine("4 - Establecer Ruta de la Llave Privada")
        Console.WriteLine("5 - Establecer Contraseña de la Llave Privada")
        Console.WriteLine("6 - Establecer Ruta de Formato Personalizado")
        Console.WriteLine("7 - Salir" & vbCrLf)
        Console.Write("Ingrese opción >")

        Dim opcion As String = Console.ReadLine
        While opcion <> "7"
            Select Case opcion
                Case "1"
                    Console.Write("Ingrese el ID de la compañía >")
                    cia = Console.ReadLine
                    Console.Clear()
                    Console.WriteLine("*************** MENU COMPAÑIA *****************" & vbCrLf)
                    Console.WriteLine("1 - Seleccionar Compañía (Actual: " & cia & ")")
                    Console.WriteLine("2 - Agregar Compañía")
                    Console.WriteLine("3 - Establecer Ruta del CSD")
                    Console.WriteLine("4 - Establecer Ruta de la Llave Privada")
                    Console.WriteLine("5 - Establecer Contraseña de la Llave Privada")
                    Console.WriteLine("6 - Establecer Ruta de Formato Personalizado")
                    Console.WriteLine("7 - Salir" & vbCrLf)
                Case "2"
                    Console.Write("Ingrese el ID de la nueva compañía >")
                    If Datos.UpdateDB("INSERT INTO comprobantes.dbo.CompInfo VALUES('" & Console.ReadLine & "', '1', '1', '1', '1')") Then
                        Console.WriteLine("Compañía agregada." & vbCrLf)
                    End If
                Case "3"
                    Console.Write("Ingrese Ruta del CSD >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.CompInfo SET CSD='" & Console.ReadLine & "' WHERE Company='" & cia & "'") Then
                        Console.WriteLine("Ruta del CSD establecida." & vbCrLf)
                    End If
                Case "4"
                    Console.Write("Ingrese Ruta de la Llave >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.CompInfo SET RutaKey='" & Console.ReadLine & "' WHERE Company='" & cia & "'") Then
                        Console.WriteLine("Ruta de la Llave establecida." & vbCrLf)
                    End If
                Case "5"
                    Console.Write("Ingrese Contraseña de la Llave >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.CompInfo SET Password='" & Console.ReadLine & "' WHERE Company='" & cia & "'") Then
                        Console.WriteLine("Contraseña de la Llave establecida." & vbCrLf)
                    End If
                Case "6"
                    Console.Write("Ingrese Ruta del Formato >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.CompInfo SET RutaCFDIper='" & Console.ReadLine & "' WHERE Company='" & cia & "'") Then
                        Console.WriteLine("Ruta del Formato establecida." & vbCrLf)
                    End If
            End Select

            Console.Write("Ingrese opción >")
            opcion = Console.ReadLine
        End While
    End Sub

    Private Sub MenuConfig()
        Dim modo As Boolean = CBool(GetSetting("MM", "Datos", "Modo", "1"))
        Console.Clear()
        Console.WriteLine("*************** MENU CONFIGURACION *****************" & vbCrLf)
        Console.WriteLine(" 1 - Activar Sistema")
        Console.WriteLine(" 2 - Establecer Servidor de Base de Datos")
        Console.WriteLine(" 3 - Establecer Usuario de Base de Datos")
        Console.WriteLine(" 4 - Establecer Contraseña de Base de Datos")
        Console.WriteLine(" 5 - Establecer Ruta de Archivos")
        Console.WriteLine(" 6 - Establecer Cuenta de Correo")
        Console.WriteLine(" 7 - Establecer Contraseña de Correo")
        Console.WriteLine(" 8 - Establecer Servidor SMTP")
        Console.WriteLine(" 9 - Establecer Puerto de Salida SMTP")
        Console.WriteLine("10 - Establecer Modo de Prueba " & modo.ToString)
        Console.WriteLine("11 - Salir" & vbCrLf)
        Console.Write("Ingrese opción >")

        Dim ruta As String = GetSetting("MM", "Datos", "RutaDocs", "Sin establecer...")
        Dim server As String = GetSetting("MM", "DATABASE", "SERVER", "Sin establecer...")
        Dim usuario As String = GetSetting("MM", "DATABASE", "USER", "Sin establecer...")
        Dim password As String = GetSetting("MM", "DATABASE", "PASSWORD", "Sin establecer...")
        Datos.ConnectionString = GetSetting("MM", "DATABASE", "CONNSTR", "Sin establecer...")
        Dim opcion As String = Console.ReadLine

        While opcion <> "11"
            Select Case opcion
                Case "1"
                    Dim NoUsuarios As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoUsuarios", String.Empty))
                    Dim NoSerie As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoSerie", String.Empty))
                    Dim NoLicencia As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoLicencia", String.Empty))
                    Dim NoHDKey As String = Datos.Desencripta(GetSetting("MM", "Datos", "NoHD", String.Empty))
                    Dim HDSerial As String = ""

                    Dim fso As New FileSystemObject
                    Dim HD As Drive = fso.GetDrive("C:\")
                    If HD.IsReady Then
                        HDSerial = HD.SerialNumber.ToString
                    Else
                        Console.WriteLine("¡El dispositivo no está listo!" & vbCrLf & "Sin esto no podrá autorizar la licencia del producto.")
                    End If

                    If NoSerie = "" Or NoLicencia = "" Or NoUsuarios = "" Or NoHDKey <> HDSerial Then
                        Console.Write("Ingrese No. de Serie >")
                        Dim inNoSerie As String = Console.ReadLine.ToUpper
                        If inNoSerie = "" Then
                            Console.WriteLine("¡No se ingresó el Número de Serie!" & vbCrLf & "Sin este número no podrá autorizar la licencia del producto.")
                        End If

                        Try
                            Dim wsLicencia As New LicenciasSAF.Service1
                            Dim ds As DataSet = wsLicencia.PermLicencia(inNoSerie, HDSerial)
                            Dim fila As DataRow = ds.Tables(0).Rows(0)
                            If Trim(fila("Mensaje")) = "AUTORIZADO" Then
                                SaveSetting("MM", "Datos", "NoSerie", Datos.Encripta(inNoSerie))
                                SaveSetting("MM", "Datos", "NoLicencia", Datos.Encripta(fila("LICENCIA").ToString))
                                SaveSetting("MM", "Datos", "NoUsuarios", Datos.Encripta(fila("NO_USUARIOS")))
                                SaveSetting("MM", "Datos", "NoHD", Datos.Encripta(HDSerial))
                                Datos.Usuarios = fila("NO_USUARIOS")
                                Console.WriteLine("Equipo Autorizado")
                            Else
                                Console.WriteLine(fila("Mensaje"))
                            End If
                        Catch ex As Exception
                            Console.WriteLine("¡No se pudo conectar con el servidor!" & vbCrLf & "Sin esto no podrá autorizar la licencia del producto.")
                        End Try
                    Else
                        Datos.Usuarios = NoUsuarios
                    End If
                Case "2"
                    Console.Write("Ingrese Servidor >")
                    server = Console.ReadLine()
                    SaveSetting("MM", "DATABASE", "SERVER", server)
                    Console.WriteLine("Servidor establecido." & vbCrLf)
                Case "3"
                    Console.Write("Ingrese Usuario >")
                    usuario = Console.ReadLine()
                    SaveSetting("MM", "DATABASE", "USER", usuario)
                    Console.WriteLine("Usuario establecido." & vbCrLf)
                Case "4"
                    Console.Write("Ingrese Contraseña >")
                    password = Console.ReadLine()
                    SaveSetting("MM", "DATABASE", "PASSWORD", password)
                    Console.WriteLine("Contraseña establecida." & vbCrLf)
                Case "5"
                    Console.Write("Ingrese Ruta >")
                    ruta = Console.ReadLine()
                    SaveSetting("MM", "Datos", "RutaDocs", ruta)
                    Datos.RutaDocs = ruta
                    Console.WriteLine("Ruta establecida." & vbCrLf)
                Case "6"
                    Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(SysConfig_Id) FROM comprobantes.dbo.SysConfig")
                    rdrCount.Read()
                    If rdrCount(0) = 0 Then
                        Datos.UpdateDB("INSERT INTO comprobantes.dbo.SysConfig (Mail, Password, host, Puerto) VALUES('','','',0)")
                    End If
                    Console.Write("Ingrese Correo >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.SysConfig SET Mail='" & Console.ReadLine & "'") Then
                        Console.WriteLine("Cuenta de Correo establecida." & vbCrLf)
                    End If
                Case "7"
                    Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(SysConfig_Id) FROM comprobantes.dbo.SysConfig")
                    rdrCount.Read()
                    If rdrCount(0) = 0 Then
                        Datos.UpdateDB("INSERT INTO comprobantes.dbo.SysConfig (Mail, Password, host, Puerto) VALUES('','','',0)")
                    End If
                    Console.Write("Ingrese Contraseña >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.SysConfig SET Password='" & Console.ReadLine & "'") Then
                        Console.WriteLine("Contraseña de Correo establecida." & vbCrLf)
                    End If
                Case "8"
                    Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(SysConfig_Id) FROM comprobantes.dbo.SysConfig")
                    rdrCount.Read()
                    If rdrCount(0) = 0 Then
                        Datos.UpdateDB("INSERT INTO comprobantes.dbo.SysConfig (Mail, Password, host, Puerto) VALUES('','','',0)")
                    End If
                    Console.Write("Ingrese Servidor SMTP >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.SysConfig SET host='" & Console.ReadLine & "'") Then
                        Console.WriteLine("Servidor SMTP de Correo establecido." & vbCrLf)
                    End If
                Case "9"
                    Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(SysConfig_Id) FROM comprobantes.dbo.SysConfig")
                    rdrCount.Read()
                    If rdrCount(0) = 0 Then
                        Datos.UpdateDB("INSERT INTO comprobantes.dbo.SysConfig (Mail, Password, host, Puerto) VALUES('','','',0)")
                    End If
                    Console.Write("Ingrese Puerto SMTP >")
                    If Datos.UpdateDB("UPDATE comprobantes.dbo.SysConfig SET Puerto=" & Console.ReadLine) Then
                        Console.WriteLine("Puerto de Salida SMTP de Correo establecido." & vbCrLf)
                    End If
                Case "10"
                    Console.Write("Ingrese Modo >")
                    modo = Console.ReadLine()
                    SaveSetting("MM", "Datos", "Modo", modo)
                    TestFlag = modo
                    Console.WriteLine("Modo establecido." & vbCrLf)
            End Select

            Datos.ConnectionString = "Data Source=" & server & ";Persist Security Info=True;User ID=" & usuario & ";Password=" & password
            SaveSetting("MM", "DATABASE", "CONNSTR", Datos.ConnectionString)
            Console.Write("Ingrese opción >")
            opcion = Console.ReadLine
        End While
    End Sub

    Private Sub CancelaCFDI(ByVal IV As String, ByVal Emp As String, ByVal motivo As String, ByVal uuid As String, ByVal opc As String)
        Empresa = Emp
        Console.WriteLine("Buscando el documento")
        Dim timbre As XmlAttributeCollection
        'Dim rdrPDF As SqlDataReader = Datos.RegresaReader("SELECT XML FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Emp & "' AND Estatus='T'")
        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT (A.COMPDES+ ' ' + A.GLFO_COMPNOM) AS COMPDES, A.ADDRESS, A.PHONE, A.FAX, A.VATNUM, A.EMAIL, A.WEBSITE, A.GLFO_NUMERO, A.GLFO_MUNICIPIO, A.GLFO_COLONIA,A.GLFO_DELEG,A.STATE AS STATENAME, E.COUNTRYNAME, B.EXTFILENAME, A.ZIP " &
                                                                "FROM " & Empresa & ".dbo.COMPDATA AS A, " & Empresa & ".dbo.COUNTRIES AS E, " & Empresa & ".dbo.COMPDATAA AS B " &
                                                                "WHERE A.COMP=-1 AND A.COUNTRY=E.COUNTRY AND A.COMP=B.COMP")

        rdrReceptor = Datos.RegresaReader("SELECT A.IV, A.IVNUM,B.VATNUM, (B.CUSTDES + ' ' + B.GLFO_COMPNOM) AS CUSTDES, B.CUST, B.ADDRESS, B.GLFO_NUMERO, B.GLFO_MUNICIPIO,B.GLFO_COLONIA,B.GLFO_DELEG, B.GLFO_NOCUENTA, B.GLFO_CONDPAGO, B.GLOB_EMAIL,C.STATENAME, D.COUNTRYNAME, C2.EMAIL, B.ZIP, P.PAYDES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES AS A, " & Empresa & ".dbo.CUSTOMERS AS B, " & Empresa & ".dbo.STATES AS C, " & Empresa & ".dbo.COUNTRIES AS D, " & Empresa & ".dbo.CUSTOMERSA C2, " & Empresa & ".dbo.PAY P " &
                                                                "WHERE A.CUST = B.CUST AND A.IV=" & IV & " AND B.STATEID=C.STATEID AND B.COUNTRY = D.COUNTRY AND C2.CUST = B.CUST AND B.PAY=P.PAY ")

        rdrFactGral = Datos.RegresaReader("SELECT I.IV,I.IVNUM, I.GLFO_CANCELPEND, I.GLFO_FACTORAJE, I.ORD, I.GLFO_OTRASFE, I.DOC, CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE) AS FECHA_CREACION, " &
                                                                "I.QPRICE, I.DISCOUNT, I.DISPRICE, I.VAT, I.TOTPRICE, I.VATPRICE, I.WTAX, I.AFTERWTAX, I.GLFO_USOCFDI, I.GLFO_METODOP, I.GLOB_FORMAPAGO, I.GLFO_SUSTITUYE, I.GLFO_IVCANCELA, I.GLFO_TRELACION, CU.CODE, CU.NAME, CU.EXCHANGE, I.DEBIT, I.IVREF, I.FINAL, I.GLFO_ADUANA, I.GLFO_FOLGR, CAST(DATEADD(DAY, I.GLFO_FECGR/1440, '1988-01-01') AS DATE) AS GLFO_FECGR, I.GLFO_TIENDAE, O.REFERENCE, CAST(DATEADD(DAY, O.GLFO_REFDATE/1440, '1988-01-01') AS DATE) AS GLFO_REFDATE, " &
                                                                "CAST(DATEADD(DAY, I.GLFO_FECPED/1440, '1988-01-01') AS DATE) AS GLFO_FECPED, I.GLFO_NPEDI, P.PAYDES, P.PAYCODE, " &
                                                                "I.FNCTRANS, I.TYPE, CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE) AS MARCA_D_TIEMPO, I.GLBF_ORDENCOMPRA, CAST(DATEADD(DAY, I.GLBF_FECH_OC/1440, '1988-01-01') AS DATE) AS GLBF_FECH_OC, " &
                                                                "I.WTAX, I.WTAXPERCENT, I.STORNOFLAG, T.TAXDES, T.TAXPERCENT, BR.BRANCHDES, (BR.ADDRESS+ ' ' + BR.GLFO_COMPNOM) AS ADDRESS, " &
                                                                "BR.STATE, CR.COUNTRYNAME, BR.ZIP, BR.PHONE, I.DETAILS, " &
                                                                "I.T$PERCENT, CAST(DATEADD(DAY, I.BALDATE/1440, '1988-01-01') AS DATE) AS FECHA_TRANSACCION, " &
                                                                "(SELECT GLFO.OBS1+'|'+GLFO.OBS2+'|'+GLFO.OBS3+'|'+GLFO.OBS4+'|'+GLFO.OBS5 FROM  " & Empresa & ".dbo.GLFO_OBSFACTURAS GLFO, " & Empresa & ".dbo.INVOICES I WHERE GLFO.IV = I.IV AND I.IV = " & IV & ") AS OBSERVACIONES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.PAY P, " & Empresa & ".dbo.TAXES T, " & Empresa & ".dbo.BRANCHES BR, " & Empresa & ".dbo.COUNTRIES CR, " &
                                                                "" & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.ORDERS O WHERE I.IV =" & IV & " AND I.FINAL = 'Y' AND I.PAY=P.PAY " &
                                                                "and I.TAX=T.TAX AND I.BRANCH=BR.BRANCH AND BR.COUNTRY=CR.COUNTRY AND CU.CURRENCY=I.CURRENCY AND I.ORD=O.ORD")


        rdrCSD.Read()
        'rdrPDF.Read()
        rdrEmisor.Read()
        rdrReceptor.Read()
        rdrFactGral.Read()
        Dim rdrPDF As SqlDataReader
        'If rdrFactGral("GLFO_CANCELPEND") <> "Y" Then
        rdrPDF = Datos.RegresaReader("SELECT XML FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Emp & "' AND Estatus='T'")
        rdrPDF.Read()

        If Not rdrPDF.HasRows Then
            rdrPDF = Datos.RegresaReader("SELECT XML FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Emp & "' AND Estatus='P'")
            rdrPDF.Read()
            If Not rdrPDF.HasRows Then
                rdrPDF = Datos.RegresaReader("SELECT XML FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Emp & "'")
                rdrPDF.Read()
                If Not rdrPDF.HasRows Then
                    Console.WriteLine("No se encontró el documento!" & vbCrLf & "Presione enter para continuar...")
                    Console.Read()
                Else
                    Console.WriteLine("El CFDI ya se encuantra cancelado!" & vbCrLf & "Presione enter para continuar...")
                    Console.Read()
                End If

                rdrPDF.Close()
                rdrCSD.Close()
                rdrEmisor.Close()
                Exit Sub
            End If
        End If
        Dim total As Double
        Try
            Console.WriteLine("Obteniendo datos del folio fiscal")
            Doc = New XmlDocument
            Doc.Load(rdrPDF.GetSqlXml(0).CreateReader())

            If rdrFactGral("TYPE") = "T" Then
                timbre = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).ChildNodes(1).Attributes
            Else
                timbre = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).FirstChild.Attributes
            End If
            total = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes("Total").Value

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.Read()
            rdrPDF.Close()
            rdrCSD.Close()
            rdrEmisor.Close()
            rdrReceptor.Close()
            Console.Read()
            Exit Sub
        End Try
        If opc = 1 Then
            Datos.CancelaSW(timbre("UUID").Value, total, IV, Emp, motivo, uuid)
        ElseIf opc = 2 Then
            Datos.StatusSAT(timbre("UUID").Value, total, IV, Emp)
        End If
        Console.Read()

        'If Datos.CancelaFinkok(New String() {timbre("UUID").Value}, total) Then
        '    If rdrFactGral("GLFO_CANCELPEND") <> "Y" Then
        '        Datos.UpdateDB("UPDATE comprobantes.dbo.Comprobante SET Estatus='P' WHERE IV=" & IV & " AND Company='" & Emp & "'")
        '        Console.Read()
        '    Else
        '        If Datos.UpdateDB("UPDATE comprobantes.dbo.Comprobante SET Estatus='C' WHERE IV=" & IV & " AND Company='" & Emp & "'") Then
        '            Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLOB_CANCELADA='Y' WHERE IV=" & rdrFactGral("IV"))
        '            Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLFO_CANCAUTH='Y' WHERE IV=" & rdrFactGral("IV"))
        '            Console.WriteLine("El CFDI se canceló exitosamente.")
        '            Console.WriteLine("Presiona enter para continuar....")
        '            Console.Read()
        '        End If
        '    End If
        'End If

        rdrPDF.Close()
        rdrCSD.Close()
        rdrEmisor.Close()
        rdrReceptor.Close()
    End Sub

    Private Sub RecuperaCFDI()
        Console.WriteLine("----------------RECUPERACIÓN DE CFDI------------------")
        Console.Write("Ingrese el nombre corto de la empresa: ")
        Empresa = Console.ReadLine
        Console.Write(Empresa & " - Ingrese el RFC del Emisor: ")
        Dim RFCEmisor As String = Console.ReadLine.ToUpper
        Console.Write(Empresa & ", " & RFCEmisor & " - Ingrese el RFC del Receptor: ")
        Dim RFCReceptor As String = Console.ReadLine.ToUpper
        Console.Write(Empresa & ", " & RFCEmisor & ", " & RFCReceptor & " - Ingrese la fecha inicial (ddmmaa): ")
        Dim FechaIni As String = Console.ReadLine
        Console.Write(Empresa & ", " & RFCEmisor & ", " & RFCReceptor & ", " & FechaIni & " - Ingrese la fecha final (ddmmaa): ")
        Dim FechaFin As String = Console.ReadLine

        Try
            FechaIni = Format(CDate(FechaIni.Substring(0, 2) & "/" & FechaIni.Substring(2, 2) & "/" & FechaIni.Substring(4, 2)), "yyyy-MM-dd")
            FechaFin = Format(CDate(FechaFin.Substring(0, 2) & "/" & FechaFin.Substring(2, 2) & "/" & FechaFin.Substring(4, 2)), "yyyy-MM-dd")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.ReadLine()
            Exit Sub
        End Try

        Dim rdrPDF As SqlDataReader = Datos.RegresaReader("SELECT PDF, XML, Estatus FROM comprobantes.dbo.Comprobante WHERE Company = '" & Empresa & "' AND Fecha BETWEEN '" & FechaIni & "' AND '" & FechaFin & "'")

        Console.WriteLine("Generando archivos PDF y XML")
        'Dim x As Integer = 1

        While rdrPDF.Read
            Try
                'Console.WriteLine(x.ToString)
                'x += 1
                Doc = New XmlDocument
                Doc.Load(rdrPDF.GetSqlXml(1).CreateReader())
                'Console.WriteLine("2")
                Doc.InsertBefore(Doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty), Doc.DocumentElement)
                'Console.WriteLine("3")
                If Doc.GetElementsByTagName("Receptor", "http://www.sat.gob.mx/cfd/3")(0).Attributes("rfc").Value = RFCReceptor Or RFCReceptor = "*" Then
                    'Console.WriteLine("4")
                    Dim RFCReceptor1 = Doc.GetElementsByTagName("Receptor", "http://www.sat.gob.mx/cfd/3")(0).Attributes("rfc").Value
                    Dim Folio As String = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes("folio").Value
                    Dim Serie As String = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes("serie").Value
                    Dim FechaDoc As String = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes("fecha").Value
                    Dim rutaXML As String = Datos.RutaDocs & "\Recuperacion\" & RFCEmisor & IIf(rdrPDF("Estatus") = "T", "\Vigentes\", "\Canceladas\") & RFCReceptor1 & "\" & Format(CDate(FechaDoc), "yyyy-MM-dd") & "\" & RFCReceptor1 & Serie & Folio & ".xml"
                    Dim rutaPDF As String = Datos.RutaDocs & "\Recuperacion\" & RFCEmisor & IIf(rdrPDF("Estatus") = "T", "\Vigentes\", "\Canceladas\") & RFCReceptor1 & "\" & Format(CDate(FechaDoc), "yyyy-MM-dd") & "\" & RFCReceptor1 & Serie & Folio & ".pdf"
                    If System.IO.File.Exists(rutaXML) Then System.IO.File.Delete(rutaXML)
                    If System.IO.File.Exists(rutaPDF) Then System.IO.File.Delete(rutaPDF)
                    System.IO.Directory.CreateDirectory(Datos.RutaDocs & "\Recuperacion\" & RFCEmisor & IIf(rdrPDF("Estatus") = "T", "\Vigentes\", "\Canceladas\") & RFCReceptor1 & "\" & Format(CDate(FechaDoc), "yyyy-MM-dd"))
                    System.IO.File.WriteAllBytes(rutaPDF, rdrPDF("PDF"))
                    Doc.Save(rutaXML)
                    Console.WriteLine(rutaXML)
                End If
            Catch ex As Exception
                Exit Try
            End Try
        End While

        'Console.WriteLine("Sali")
        'Console.ReadLine()

        rdrPDF.Close()

    End Sub

    Private Sub ReenviarCFDI(ByVal IV As String, ByVal Emp As String)
        Empresa = Emp

        Console.WriteLine("Buscando documento")

        rdrEmisor = Datos.RegresaReader("SELECT (A.COMPDES+ ' ' + A.GLFO_COMPNOM) AS COMPDES, A.ADDRESS, A.PHONE, A.FAX, A.VATNUM, A.GLFO_REGIMEN, A.EMAIL, A.WEBSITE, A.GLFO_NUMERO, A.GLFO_MUNICIPIO, A.GLFO_COLONIA,A.GLFO_DELEG,A.STATE AS STATENAME, E.COUNTRYNAME, B.EXTFILENAME, A.ZIP " &
                                                                "FROM " & Empresa & ".dbo.COMPDATA AS A, " & Empresa & ".dbo.COUNTRIES AS E, " & Empresa & ".dbo.COMPDATAA AS B " &
                                                                "WHERE A.COMP=-1 AND A.COUNTRY=E.COUNTRY AND A.COMP=B.COMP")

        rdrReceptor = Datos.RegresaReader("SELECT A.IV, A.IVNUM,B.VATNUM, (B.CUSTDES + ' ' + B.GLFO_COMPNOM) AS CUSTDES, B.CUST, B.ADDRESS, B.GLFO_NUMERO, B.GLFO_MUNICIPIO,B.GLFO_COLONIA,B.GLFO_DELEG, B.GLFO_NOCUENTA, B.GLFO_CONDPAGO, B.GLOB_EMAIL, B.GLFO_NUMPRO, B.GLFO_TIPOP, B.GLFO_GLN1, B.GLFO_GLN2, B.GLOB_ADENDAAMC,C.STATENAME, D.COUNTRYNAME, C2.EMAIL, B.ZIP, P.PAYDES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES AS A, " & Empresa & ".dbo.CUSTOMERS AS B, " & Empresa & ".dbo.STATES AS C, " & Empresa & ".dbo.COUNTRIES AS D, " & Empresa & ".dbo.CUSTOMERSA C2, " & Empresa & ".dbo.PAY P " &
                                                                "WHERE A.CUST = B.CUST AND A.IV=" & IV & " AND B.STATEID=C.STATEID AND B.COUNTRY = D.COUNTRY AND C2.CUST = B.CUST AND B.PAY=P.PAY ")

        rdrFactGral = Datos.RegresaReader("SELECT I.IV,I.IVNUM, I.ORD, I.GLFO_OTRASFE, I.DOC, CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE)AS FECHA_CREACION, " &
                                                                "I.QPRICE, I.DISCOUNT, I.DISPRICE, I.VAT, I.TOTPRICE, I.VATPRICE, I.WTAX, I.AFTERWTAX, CU.CODE, CU.NAME, CU.EXCHANGE, I.DEBIT, I.IVREF, I.FINAL, I.GLFO_ADUANA, I.GLFO_FOLGR, CAST(DATEADD(DAY, I.GLFO_FECGR/1440, '1988-01-01') AS DATE) AS GLFO_FECGR, I.GLFO_TIENDAE, O.REFERENCE, CAST(DATEADD(DAY, O.GLFO_REFDATE/1440, '1988-01-01') AS DATE) AS GLFO_REFDATE, " &
                                                                "CAST(DATEADD(DAY, I.GLFO_FECPED/1440, '1988-01-01') AS DATE) AS GLFO_FECPED, I.GLFO_NPEDI, P.PAYDES, P.PAYCODE, " &
                                                                "I.FNCTRANS, I.TYPE, CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE) AS MARCA_D_TIEMPO, I.GLBF_ORDENCOMPRA, CAST(DATEADD(DAY, I.GLBF_FECH_OC/1440, '1988-01-01') AS DATE) AS GLBF_FECH_OC, " &
                                                                "I.WTAX, I.WTAXPERCENT, I.STORNOFLAG, T.TAXDES, T.TAXPERCENT, BR.BRANCHDES, (BR.ADDRESS+ ' ' + BR.GLFO_COMPNOM) AS ADDRESS, " &
                                                                "BR.STATE, CR.COUNTRYNAME, BR.ZIP, BR.PHONE, I.DETAILS, " &
                                                                "I.T$PERCENT, CAST(DATEADD(DAY, I.BALDATE/1440, '1988-01-01') AS DATE) AS FECHA_TRANSACCION, " &
                                                                "(SELECT GLFO.OBS1+'|'+GLFO.OBS2+'|'+GLFO.OBS3+'|'+GLFO.OBS4+'|'+GLFO.OBS5 FROM  " & Empresa & ".dbo.GLFO_OBSFACTURAS GLFO, " & Empresa & ".dbo.INVOICES I WHERE GLFO.IV = I.IV AND I.IV = " & IV & ") AS OBSERVACIONES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.PAY P, " & Empresa & ".dbo.TAXES T, " & Empresa & ".dbo.BRANCHES BR, " & Empresa & ".dbo.COUNTRIES CR, " &
                                                                "" & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.ORDERS O WHERE I.IV =" & IV & " AND I.FINAL = 'Y' AND I.PAY=P.PAY " &
                                                                "and I.TAX=T.TAX AND I.BRANCH=BR.BRANCH AND BR.COUNTRY=CR.COUNTRY AND CU.CURRENCY=I.CURRENCY AND I.ORD=O.ORD")

        rdrEmisor.Read()
        rdrReceptor.Read()
        rdrFactGral.Read()

        Dim rutaXML As String = Datos.RutaDocs & "\" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & ".xml"
        Dim rutaPDF As String = Datos.RutaDocs & "\" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & ".pdf"
        Dim rdrPDF As SqlDataReader = Datos.RegresaReader("SELECT PDF, XML FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Emp & "'AND Estatus='T'")
        rdrPDF.Read()

        If rdrPDF.HasRows Then
            Console.WriteLine("Generando archivos PDF y XML")
            If System.IO.File.Exists(rutaXML) Then System.IO.File.Delete(rutaXML)
            If System.IO.File.Exists(rutaPDF) Then System.IO.File.Delete(rutaPDF)
            Try
                Doc = New XmlDocument
                Doc.Load(rdrPDF.GetSqlXml(1).CreateReader())
                Doc.InsertBefore(Doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty), Doc.DocumentElement)
                Doc.Save(rutaXML)
                System.IO.File.WriteAllBytes(rutaPDF, rdrPDF("PDF"))
            Catch ex As Exception
                Console.WriteLine("Error al cargar el documento!" & vbCrLf & "Presione cualquier tecla para continuar...")
                Console.Read()
            End Try

            rdrPDF.Close()
        Else
            Console.WriteLine("No se encontró el documento!" & vbCrLf & "Presione cualquier tecla para continuar...")
            Console.Read()
            rdrPDF.Close()
            rdrEmisor.Close()
            rdrReceptor.Close()
            rdrFactGral.Close()
            Console.Read()
            Exit Sub
        End If

        EnviaCorreo(rutaXML)
        If System.IO.File.Exists(rutaXML) Then System.IO.File.Delete(rutaXML)
        If System.IO.File.Exists(rutaPDF) Then System.IO.File.Delete(rutaPDF)
        rdrEmisor.Close()
        rdrReceptor.Close()
        rdrFactGral.Close()
    End Sub

    Private Sub GeneraRecibo3_3(ByVal IV As String, ByVal Emp As String)
        Empresa = Emp
        reciboPago = True
        Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(Comprobante_Id) AS Cuenta FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Empresa & "'")
        rdrCount.Read()
        If rdrCount("Cuenta") > 0 Then
            Console.WriteLine("El comprobante ya está generado")
            VerCFDI(IV, Emp)
            Exit Sub
        End If

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT (A.COMPDES+ ' ' + A.GLFO_COMPNOM) AS COMPDES, A.ADDRESS, A.PHONE, A.FAX, A.VATNUM, A.GLFO_REGIMEN, A.EMAIL, A.WEBSITE, A.GLFO_NUMERO, A.GLFO_MUNICIPIO, A.GLFO_COLONIA,A.GLFO_DELEG,A.STATE AS STATENAME, E.COUNTRYNAME, B.EXTFILENAME, A.ZIP " &
                                                                "FROM " & Empresa & ".dbo.COMPDATA AS A, " & Empresa & ".dbo.COUNTRIES AS E, " & Empresa & ".dbo.COMPDATAA AS B " &
                                                                "WHERE A.COMP=-1 AND A.COUNTRY=E.COUNTRY AND A.COMP=B.COMP")

        rdrReceptor = Datos.RegresaReader("SELECT A.IV, A.IVNUM,B.VATNUM, (B.CUSTDES + ' ' + B.GLFO_COMPNOM) AS CUSTDES, B.CUST, B.ADDRESS, B.GLFO_NUMERO, B.GLFO_MUNICIPIO,B.GLFO_COLONIA,B.GLFO_DELEG, B.GLFO_NOCUENTA, B.GLFO_CONDPAGO, B.GLOB_EMAIL, B.GLFO_NUMPRO, B.GLFO_TIPOP, B.GLFO_GLN1, B.GLFO_GLN2, B.GLOB_ADENDAAMC,C.STATENAME, D.COUNTRYNAME, D.COUNTRYCODE, C2.EMAIL, B.ZIP, P.PAYDES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES AS A, " & Empresa & ".dbo.CUSTOMERS AS B, " & Empresa & ".dbo.STATES AS C, " & Empresa & ".dbo.COUNTRIES AS D, " & Empresa & ".dbo.CUSTOMERSA C2, " & Empresa & ".dbo.PAY P " &
                                                                "WHERE A.CUST = B.CUST AND A.IV=" & IV & " AND B.STATEID=C.STATEID AND B.COUNTRY = D.COUNTRY AND C2.CUST = B.CUST AND B.PAY=P.PAY ")

        rdrFactGral = Datos.RegresaReader("SELECT I.IV,I.IVNUM, I.ORD, I.GLFO_FACTORAJE,CL.Nombre, CL.RFC, I.GLFO_CLIENTEFACT, CAST(DATEADD(DAY, I.GLFO_FECPAGO/1440, '1988-01-01') AS DATE) AS GLFO_FECPAGO, I.GLFO_OTRASFE, I.DOC, CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE) AS FECHA_CREACION, " &
                                                                "I.QPRICE, I.DISCOUNT, I.DISPRICE, I.VAT, I.TOTPRICE, I.VATPRICE, I.WTAX, I.AFTERWTAX, I.GLFO_USOCFDI, I.GLFO_METODOP, I.GLOB_FORMAPAGO, I.GLFO_SUSTITUYE, I.GLFO_IVCANCELA, I.GLFO_TRELACION, CU.CODE, CU.NAME, CU.EXCHANGE, I.DEBIT, I.IVREF, I.FINAL, I.GLFO_ADUANA, I.GLFO_FOLGR, CAST(DATEADD(DAY, I.GLFO_FECGR/1440, '1988-01-01') AS DATE) AS GLFO_FECGR, I.GLFO_TIENDAE, O.REFERENCE, CAST(DATEADD(DAY, O.GLFO_REFDATE/1440, '1988-01-01') AS DATE) AS GLFO_REFDATE, " &
                                                                "CAST(DATEADD(DAY, I.GLFO_FECPED/1440, '1988-01-01') AS DATE) AS GLFO_FECPED, I.GLFO_NPEDI, P.PAYDES, P.PAYCODE, " &
                                                                "I.FNCTRANS, I.TYPE, CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE) AS MARCA_D_TIEMPO, I.GLBF_ORDENCOMPRA, CAST(DATEADD(DAY, I.GLBF_FECH_OC/1440, '1988-01-01') AS DATE) AS GLBF_FECH_OC, " &
                                                                "I.WTAX, I.WTAXPERCENT, I.STORNOFLAG, T.TAXDES, T.TAXPERCENT, BR.BRANCHDES, (BR.ADDRESS+ ' ' + BR.GLFO_COMPNOM) AS ADDRESS, " &
                                                                "BR.STATE, CR.COUNTRYNAME, BR.ZIP, BR.PHONE, I.DETAILS, " &
                                                                "I.T$PERCENT, CAST(DATEADD(DAY, I.BALDATE/1440, '1988-01-01') AS DATE) AS FECHA_TRANSACCION, " &
                                                                "(SELECT GLFO.OBS1+'|'+GLFO.OBS2+'|'+GLFO.OBS3+'|'+GLFO.OBS4+'|'+GLFO.OBS5 FROM " & Empresa & ".dbo.GLFO_OBSFACTURAS GLFO, " & Empresa & ".dbo.INVOICES I WHERE GLFO.IV = I.IV AND I.IV = " & IV & ") AS OBSERVACIONES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES I LEFT JOIN " & Empresa & ".dbo.GLFO_CLIENTEFACT CL ON I.GLFO_CLIENTEFACT = CL.RFC, " & Empresa & ".dbo.PAY P, " & Empresa & ".dbo.TAXES T, " & Empresa & ".dbo.BRANCHES BR, " & Empresa & ".dbo.COUNTRIES CR, " &
                                                                "" & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.ORDERS O WHERE I.IV =" & IV & " AND I.FINAL = 'Y' AND I.PAY=P.PAY " &
                                                                "and I.TAX=T.TAX AND I.BRANCH=BR.BRANCH AND BR.COUNTRY=CR.COUNTRY AND CU.CURRENCY=I.CURRENCY AND I.ORD=O.ORD")

        Dim facturasPagadas As New SqlDataAdapter("SELECT  F.CREDIT1, F.CREDIT2, FAUX.GLFO_RELAC, X.IVNUM, I.IVNUM AS IVNUMFACT, X.TOTPRICE, FA.FNCIREF1, I.GLOB_FOLIOFISCAL, I.GLFO_METODOP, FB.IVBALANCE, FBA.IVBALANCE2, I.TOTPRICE AS TOTALFACTURA, CU.CODE, CU.EXCHANGE, (SELECT COUNT(1) FROM " & Empresa & ".dbo.FNCITEMSA FAX, " & Empresa & ".dbo.FNCITEMS F, " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.FNCTRANS FT " &
                                                  "WHERE FAX.FNCIREF1 = FA.FNCIREF1 AND FAX.FNCTRANS=F.FNCTRANS AND I.FNCTRANS=FAX.FNCTRANS AND F.LINE=FAX.KLINE AND FT.FNCTRANS=F.FNCTRANS AND I.TYPE = 'T' AND FT.CFNCTRANS = 0 AND F.QIV <> 0) AS  PARCIALIDAD " &
                                                  "FROM " & Empresa & ".dbo.INVOICES X, " & Empresa & ".dbo.FNCITEMS F, " & Empresa & ".dbo.FNCITEMSA FA, " & Empresa & ".dbo.FNCITEMSB FBA, " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.FNCITEMS FAUX, " & Empresa & ".dbo.FNCITEMSA FB, " & Empresa & ".dbo.CURRENCIES CU " &
                                                  "WHERE X.IV =" & IV & " AND X.FNCTRANS=F.FNCTRANS AND X.ACCOUNT = F.ACCOUNT AND FBA.FNCTRANS=FB.FNCTRANS AND FBA.KLINE=1 AND FA.FNCTRANS=X.FNCTRANS AND F.KLINE=FA.KLINE AND I.IVNUM = FA.FNCIREF1 AND FAUX.FNCTRANS = I.FNCTRANS AND FB.KLINE = FAUX.KLINE AND FB.FNCTRANS=I.FNCTRANS AND I.CURRENCY=CU.CURRENCY AND I.GLOB_FOLIOFISCAL <> '' AND FB.IVBALANCE <> 0", Datos.ConnectionString)

        rdrBanco = Datos.RegresaReader("SELECT C.PAYACCOUNT, C.BIC, C.CASHNAME FROM " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.CASH C WHERE I.IV =" & IV & " AND C.CASH = I.TOCASH")

        Try
            rdrFactPagadas = New DataTable
            facturasPagadas.Fill(rdrFactPagadas)
            rdrBanco.Read()
            rdrCSD.Read()
            rdrEmisor.Read()
            rdrReceptor.Read()
            rdrFactGral.Read()
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
            Console.Read()
        End Try


        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine(Datos.ObtieneSysInfo(rdrEmisor("VATNUM").ToString))
            Console.WriteLine("Generando Comprobante: " & rdrFactGral("IVNUM").ToString.Trim)
            Console.WriteLine(Empresa)
            GenerarComprobanteCFDI3_3() 'rdrFactGral, rdrFactItems, rdrEmisor, Empresa)
            Console.WriteLine("¡Proceso concluido!")
            VerCFDI(IV, Emp)
        Else
            Console.WriteLine("¡No se pudo generar el comprobante debido a errores en el registro de pagos!" & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrEmisor.Close()
        rdrReceptor.Close()
        rdrFactGral.Close()


    End Sub

    Private Sub GeneraCFDI3_3(ByVal IV As String, ByVal Emp As String)
        Empresa = Emp
        Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(Comprobante_Id) AS Cuenta FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Empresa & "'")
        rdrCount.Read()
        If rdrCount("Cuenta") > 0 Then
            Console.WriteLine("El comprobante ya está generado")
            VerCFDI(IV, Emp)
            Exit Sub
        End If

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT (A.COMPDES+ ' ' + A.GLFO_COMPNOM) AS COMPDES, A.ADDRESS, A.PHONE, A.FAX, A.VATNUM, A.GLFO_REGIMEN, A.EMAIL, A.WEBSITE, A.GLFO_NUMERO, A.GLFO_MUNICIPIO, A.GLFO_COLONIA,A.GLFO_DELEG,A.STATE AS STATENAME, E.COUNTRYNAME, B.EXTFILENAME, A.ZIP " &
                                                                "FROM " & Empresa & ".dbo.COMPDATA AS A, " & Empresa & ".dbo.COUNTRIES AS E, " & Empresa & ".dbo.COMPDATAA AS B " &
                                                                "WHERE A.COMP=-1 AND A.COUNTRY=E.COUNTRY AND A.COMP=B.COMP")

        rdrReceptor = Datos.RegresaReader("SELECT A.IV, A.IVNUM,B.VATNUM, (B.CUSTDES + ' ' + B.GLFO_COMPNOM) AS CUSTDES, B.CUST, B.ADDRESS, B.GLFO_NUMERO, B.GLFO_MUNICIPIO,B.GLFO_COLONIA,B.GLFO_DELEG, B.GLFO_NOCUENTA, B.GLFO_CONDPAGO, B.GLOB_EMAIL, B.GLFO_NUMPRO, B.GLFO_TIPOP, B.GLFO_GLN1, B.GLFO_GLN2, B.GLOB_ADENDAAMC,C.STATENAME, D.COUNTRYNAME, D.COUNTRYCODE, C2.EMAIL, B.ZIP, P.PAYDES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES AS A, " & Empresa & ".dbo.CUSTOMERS AS B, " & Empresa & ".dbo.STATES AS C, " & Empresa & ".dbo.COUNTRIES AS D, " & Empresa & ".dbo.CUSTOMERSA C2, " & Empresa & ".dbo.PAY P " &
                                                                "WHERE A.CUST = B.CUST AND A.IV=" & IV & " AND B.STATEID=C.STATEID AND B.COUNTRY = D.COUNTRY AND C2.CUST = B.CUST AND B.PAY=P.PAY ")

        rdrFactGral = Datos.RegresaReader("SELECT I.IV,I.IVNUM, I.GLFO_FACTORAJE, I.ORD, I.GLFO_OTRASFE, I.DOC, CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE) AS FECHA_CREACION, " &
                                                                "I.QPRICE, I.DISCOUNT, I.DISPRICE, I.VAT, I.TOTPRICE, I.VATPRICE, I.WTAX, I.AFTERWTAX, I.GLFO_USOCFDI, I.GLFO_METODOP, I.GLOB_FORMAPAGO, I.GLFO_SUSTITUYE, I.GLFO_IVCANCELA, I.GLFO_TRELACION, CU.CODE, CU.NAME, CU.EXCHANGE, I.DEBIT, I.IVREF, I.FINAL, I.GLFO_ADUANA, I.GLFO_FOLGR, CAST(DATEADD(DAY, I.GLFO_FECGR/1440, '1988-01-01') AS DATE) AS GLFO_FECGR, I.GLFO_TIENDAE, O.REFERENCE, CAST(DATEADD(DAY, O.GLFO_REFDATE/1440, '1988-01-01') AS DATE) AS GLFO_REFDATE, " &
                                                                "CAST(DATEADD(DAY, I.GLFO_FECPED/1440, '1988-01-01') AS DATE) AS GLFO_FECPED, I.GLFO_NPEDI, P.PAYDES, P.PAYCODE, " &
                                                                "I.FNCTRANS, I.TYPE, CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE) AS MARCA_D_TIEMPO, I.GLBF_ORDENCOMPRA, CAST(DATEADD(DAY, I.GLBF_FECH_OC/1440, '1988-01-01') AS DATE) AS GLBF_FECH_OC, " &
                                                                "I.WTAX, I.WTAXPERCENT, I.STORNOFLAG, T.TAXDES, T.TAXPERCENT, BR.BRANCHDES, (BR.ADDRESS+ ' ' + BR.GLFO_COMPNOM) AS ADDRESS, " &
                                                                "BR.STATE, CR.COUNTRYNAME, BR.ZIP, BR.PHONE, I.DETAILS, " &
                                                                "I.T$PERCENT, CAST(DATEADD(DAY, I.BALDATE/1440, '1988-01-01') AS DATE) AS FECHA_TRANSACCION, " &
                                                                "(SELECT GLFO.OBS1+'|'+GLFO.OBS2+'|'+GLFO.OBS3+'|'+GLFO.OBS4+'|'+GLFO.OBS5 FROM  " & Empresa & ".dbo.GLFO_OBSFACTURAS GLFO, " & Empresa & ".dbo.INVOICES I WHERE GLFO.IV = I.IV AND I.IV = " & IV & ") AS OBSERVACIONES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.PAY P, " & Empresa & ".dbo.TAXES T, " & Empresa & ".dbo.BRANCHES BR, " & Empresa & ".dbo.COUNTRIES CR, " &
                                                                "" & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.ORDERS O WHERE I.IV =" & IV & " AND I.FINAL = 'Y' AND I.PAY=P.PAY " &
                                                                "and I.TAX=T.TAX AND I.BRANCH=BR.BRANCH AND BR.COUNTRY=CR.COUNTRY AND CU.CURRENCY=I.CURRENCY AND I.ORD=O.ORD")

        Dim daItems As New SqlDataAdapter("SELECT I.IV,I.PART,IX.IVTAX,P.PARTNAME, (P.PARTDES + ' ' + I.GLB_DESC_1 + ' ' + I.GLB_DESC_2) AS PARTDES, P.GLFO_CLAVEP, I.PRICE,I.TQUANT/1000.00 AS QUANT,CU.CODE,I.LINE, " &
                                                                        "CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE)AS FECHA_CREACION, I.T$PERCENT,I.QPRICE,I.TUNIT,U.UNITNAME,U.UNITDES, TX.TAXCODE, TX.TAXPERCENT, " &
                                                                        "CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE)AS MARCA_D_TIEMPO,(select  TOP 1 SERIALNAME FROM " & Empresa & ".dbo.SERIAL S WHERE P.PART=S.PART) as PEDIMENTO, " &
                                                                        "(SELECT  TOP 1 S.ATCUST FROM " & Empresa & ".dbo.SERIAL S WHERE S.PART=P.PART) AS ATCUST, (SELECT TEXT FROM  " & Empresa & ".dbo.NONSTANDARD WHERE NONSTANDARD.NONSTANDARD = I.NONSTANDARD AND I.PART = P.PART) AS NONSTANDARD, ((SELECT TEXT FROM  " & Empresa & ".dbo.NONSTANDARD WHERE NONSTANDARD.NONSTANDARD = I.NONSTANDARD AND I.PART = P.PART) + ' ' + I.GLB_DESC_1 + ' ' + I.GLB_DESC_2) AS PARTDES2, " &
                                                                        "(SELECT node.text FROM (SELECT DISTINCT(IV) FROM " & Empresa & ".dbo.INVOICEITEMSTEXT WHERE IV=" & IV & " AND I.KLINE = KLINE) AS IT CROSS APPLY(SELECT (TEXT + ' ') AS '*' FROM " & Empresa & ".dbo.INVOICEITEMSTEXT WHERE IV=" & IV & " AND I.KLINE = KLINE FOR XML PATH('')) AS node(text))  AS OBSERVACIONES " &
                                                                        "FROM " & Empresa & ".dbo.INVOICEITEMS I, " & Empresa & ".dbo.INVOICEITEMSA IX, " & Empresa & ".dbo.PART P, " & Empresa & ".dbo.UNIT U, " & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.TAXES TX " &
                                                                        "WHERE I.PART = P.PART AND I.IV = IX.IV AND I.KLINE=IX.KLINE AND P.PUNIT = U.UNIT AND I.IV=" & IV & " AND I.CURRENCY=CU.CURRENCY AND TX.TAX = IX.WTAXTBL", Datos.ConnectionString)

        rdrFactItems = New DataTable
        daItems.Fill(rdrFactItems)

        rdrCSD.Read()
        rdrEmisor.Read()
        rdrReceptor.Read()
        rdrFactGral.Read()
        'rdrFactItems.Read()
        If rdrFactGral("GLFO_SUSTITUYE") = "Y" Then
            rdrFactRel = Datos.RegresaReader("SELECT GLOB_FOLIOFISCAL FROM " & Empresa & ".dbo.INVOICES WHERE IVNUM='" & rdrFactGral("GLFO_IVCANCELA") & "'")
            rdrFactRel.Read()
        End If

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine(Datos.ObtieneSysInfo(rdrEmisor("VATNUM").ToString))
            Console.WriteLine("Generando Comprobante: " & rdrFactGral("IVNUM").ToString.Trim)
            Console.WriteLine(Empresa)
            GenerarComprobanteCFDI3_3() 'rdrFactGral, rdrFactItems, rdrEmisor, Empresa)
            Console.WriteLine("¡Proceso concluido!")
            VerCFDI(IV, Emp)
        Else
            Console.WriteLine("¡No se pudo generar el comprobante debido a errores en el registro de pagos!" & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrEmisor.Close()
        rdrReceptor.Close()
        rdrFactGral.Close()


    End Sub

    Private Function Observaciones(cadena As String) As String
        Dim str As String = ""
        Try
            Dim obs0 As String = Regex.Replace(cadena, "&lt;/P&gt;", "")
            Dim obs1 As String = Regex.Replace(obs0, "=&gt;", "=>")
            Dim obs2 As String = Regex.Replace(obs1, "&lt;style&gt;.*?&lt;/style&gt;", "")
            Dim obs3 As String = Regex.Replace(obs2, "&lt;.*?&gt;", "")
            str = Regex.Replace(obs3, "&amp;nbsp;", "")
        Catch ex As Exception

        End Try
        Return str
    End Function

    Private Sub GeneraCFDI(ByVal IV As String, ByVal Emp As String)
        Empresa = Emp
        Dim rdrCount As SqlDataReader = Datos.RegresaReader("SELECT COUNT(Comprobante_Id) AS Cuenta FROM comprobantes.dbo.Comprobante WHERE IV=" & IV & " AND Company='" & Empresa & "'")
        rdrCount.Read()
        If rdrCount("Cuenta") > 0 Then
            Console.WriteLine("El comprobante ya está generado")
            VerCFDI(IV, Emp)
            Exit Sub
        End If

        rdrCSD = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.CompInfo WHERE Company='" & Empresa & "'")

        rdrEmisor = Datos.RegresaReader("SELECT (A.COMPDES+ ' ' + A.GLFO_COMPNOM) AS COMPDES, A.ADDRESS, A.PHONE, A.FAX, A.VATNUM, A.GLFO_REGIMEN, A.EMAIL, A.WEBSITE, A.GLFO_NUMERO, A.GLFO_MUNICIPIO, A.GLFO_COLONIA,A.GLFO_DELEG,A.STATE AS STATENAME, E.COUNTRYNAME, B.EXTFILENAME, A.ZIP " &
                                                                "FROM " & Empresa & ".dbo.COMPDATA AS A, " & Empresa & ".dbo.COUNTRIES AS E, " & Empresa & ".dbo.COMPDATAA AS B " &
                                                                "WHERE A.COMP=-1 AND A.COUNTRY=E.COUNTRY AND A.COMP=B.COMP")

        rdrReceptor = Datos.RegresaReader("SELECT A.IV, A.IVNUM,B.VATNUM, (B.CUSTDES + ' ' + B.GLFO_COMPNOM) AS CUSTDES, B.CUST, B.ADDRESS, B.GLFO_NUMERO, B.GLFO_MUNICIPIO,B.GLFO_COLONIA,B.GLFO_DELEG, B.GLFO_NOCUENTA, B.GLFO_CONDPAGO, B.GLOB_EMAIL, B.GLFO_NUMPRO, B.GLFO_TIPOP, B.GLFO_GLN1, B.GLFO_GLN2, B.GLOB_ADENDAAMC,C.STATENAME, D.COUNTRYNAME, C2.EMAIL, B.ZIP, P.PAYDES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES AS A, " & Empresa & ".dbo.CUSTOMERS AS B, " & Empresa & ".dbo.STATES AS C, " & Empresa & ".dbo.COUNTRIES AS D, " & Empresa & ".dbo.CUSTOMERSA C2, " & Empresa & ".dbo.PAY P " &
                                                                "WHERE A.CUST = B.CUST AND A.IV=" & IV & " AND B.STATEID=C.STATEID AND B.COUNTRY = D.COUNTRY AND C2.CUST = B.CUST AND B.PAY=P.PAY ")

        rdrFactGral = Datos.RegresaReader("SELECT I.IV,I.IVNUM, I.ORD, I.GLFO_OTRASFE, I.DOC, CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE) AS FECHA_CREACION, " &
                                                                "I.QPRICE, I.DISCOUNT, I.DISPRICE, I.VAT, I.TOTPRICE, I.VATPRICE, I.WTAX, I.AFTERWTAX, CU.CODE, CU.NAME, CU.EXCHANGE, I.DEBIT, I.IVREF, I.FINAL, I.GLFO_ADUANA, I.GLFO_FOLGR, CAST(DATEADD(DAY, I.GLFO_FECGR/1440, '1988-01-01') AS DATE) AS GLFO_FECGR, I.GLFO_TIENDAE, O.REFERENCE, CAST(DATEADD(DAY, O.GLFO_REFDATE/1440, '1988-01-01') AS DATE) AS GLFO_REFDATE, " &
                                                                "CAST(DATEADD(DAY, I.GLFO_FECPED/1440, '1988-01-01') AS DATE) AS GLFO_FECPED, I.GLFO_NPEDI, P.PAYDES, P.PAYCODE, " &
                                                                "I.FNCTRANS, I.TYPE, CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE) AS MARCA_D_TIEMPO, I.GLBF_ORDENCOMPRA, CAST(DATEADD(DAY, I.GLBF_FECH_OC/1440, '1988-01-01') AS DATE) AS GLBF_FECH_OC, " &
                                                                "I.WTAX, I.WTAXPERCENT, I.STORNOFLAG, T.TAXDES, T.TAXPERCENT, BR.BRANCHDES, (BR.ADDRESS+ ' ' + BR.GLFO_COMPNOM) AS ADDRESS, " &
                                                                "BR.STATE, CR.COUNTRYNAME, BR.ZIP, BR.PHONE, I.DETAILS, " &
                                                                "I.T$PERCENT, CAST(DATEADD(DAY, I.BALDATE/1440, '1988-01-01') AS DATE) AS FECHA_TRANSACCION, " &
                                                                "(SELECT GLFO.OBS1+'|'+GLFO.OBS2+'|'+GLFO.OBS3+'|'+GLFO.OBS4+'|'+GLFO.OBS5 FROM  " & Empresa & ".dbo.GLFO_OBSFACTURAS GLFO, " & Empresa & ".dbo.INVOICES I WHERE GLFO.IV = I.IV AND I.IV = " & IV & ") AS OBSERVACIONES " &
                                                                "FROM " & Empresa & ".dbo.INVOICES I, " & Empresa & ".dbo.PAY P, " & Empresa & ".dbo.TAXES T, " & Empresa & ".dbo.BRANCHES BR, " & Empresa & ".dbo.COUNTRIES CR, " &
                                                                "" & Empresa & ".dbo.CURRENCIES CU, " & Empresa & ".dbo.ORDERS O WHERE I.IV =" & IV & " AND I.FINAL = 'Y' AND I.PAY=P.PAY " &
                                                                "and I.TAX=T.TAX AND I.BRANCH=BR.BRANCH AND BR.COUNTRY=CR.COUNTRY AND CU.CURRENCY=I.CURRENCY AND I.ORD=O.ORD")

        Dim daItems As New SqlDataAdapter("SELECT I.IV,I.PART,P.PARTNAME, (P.PARTDES + ' ' + I.GLB_DESC_1 + ' ' + I.GLB_DESC_2) AS PARTDES, I.PRICE,I.TQUANT/1000.00 AS QUANT,CU.CODE,I.LINE, " &
                                                                "CAST(DATEADD(DAY, I.IVDATE/1440, '1988-01-01') AS DATE)AS FECHA_CREACION, I.T$PERCENT,I.QPRICE,I.TUNIT,U.UNITNAME,U.UNITDES, " &
                                                                "CAST(DATEADD(DAY, I.UDATE/1440, '1988-01-01') AS DATE)AS MARCA_D_TIEMPO,(select  TOP 1 SERIALNAME FROM " & Empresa & ".dbo.SERIAL S WHERE P.PART=S.PART) as PEDIMENTO, " &
                                                                "(SELECT  TOP 1 S.ATCUST FROM " & Empresa & ".dbo.SERIAL S WHERE S.PART=P.PART) AS ATCUST, ((SELECT TEXT FROM  " & Empresa & ".dbo.NONSTANDARD WHERE NONSTANDARD.NONSTANDARD = I.NONSTANDARD AND I.PART = P.PART) + ' ' + I.GLB_DESC_1 + ' ' + I.GLB_DESC_2) AS PARTDES2 " &
                                                                "FROM " & Empresa & ".dbo.INVOICEITEMS I, " & Empresa & ".dbo.PART P, " & Empresa & ".dbo.UNIT U, " & Empresa & ".dbo.CURRENCIES CU " &
                                                                "WHERE I.PART = P.PART AND P.UNIT = U.UNIT AND IV=" & IV & " AND I.CURRENCY=CU.CURRENCY", Datos.ConnectionString)

        rdrFactItems = New DataTable
        daItems.Fill(rdrFactItems)

        rdrCSD.Read()
        rdrEmisor.Read()
        rdrReceptor.Read()
        rdrFactGral.Read()
        'rdrFactItems.Read()

        If Datos.compruebaTimbresRestantes(rdrEmisor("VATNUM").ToString) Then
            Console.WriteLine(Datos.ObtieneSysInfo(rdrEmisor("VATNUM").ToString))
            Console.WriteLine("Generando Comprobante: " & rdrFactGral("IVNUM").ToString.Trim)
            GenerarComprobanteCFDI() 'rdrFactGral, rdrFactItems, rdrEmisor, Empresa)
            Console.WriteLine("¡Proceso concluido!")
            VerCFDI(IV, Emp)
        Else
            Console.WriteLine("¡No se pudo generar el comprobante debido a errores en el registro de pagos!" & "Presione cualquier tecla para continuar...")
            Console.Read()
        End If

        rdrEmisor.Close()
        rdrReceptor.Close()
        rdrFactGral.Close()


    End Sub

    Private Sub GenerarComprobanteCFDI3_3() 'ByRef filaGen As SqlDataReader, ByRef dtDet As SqlDataReader, ByVal rdrEmisor As SqlDataReader, ByRef Empresa As String)
        Doc = New XmlDocument

        Dim Params As New FirmaParams
        Params.ArchivoXSD = "cfdv33.xsd"
        Params.ArchivoXSLT = "cadenaoriginal_3_3.xslt"
        Params.NodoCert = "Comprobante"
        Params.EspacioNombres = "http://www.sat.gob.mx/cfd/3"
        Params.AtrCertif = "Certificado"
        Params.AtrNoCert = "NoCertificado"
        Params.AtrSello = "Sello"

        Try
            If reciboPago Then
                LlenaXMLCFDIRecibo3_3()
            Else
                LlenaXMLCFDI3_3()
            End If
            Console.WriteLine("OK - CFDI creado.")
            Console.WriteLine("Firmando CFDI...")
            Datos.Firma3_3(Params)
            Console.WriteLine("OK - CFDI firmado.")
            Console.WriteLine("Validando CFDI...")
            If Datos.Valida(Params) Then
                Console.WriteLine("OK - CFDI Validado.")
                Console.WriteLine("Timbrando CFDI...")
                If Not Datos.TimbraSW() Then
                    Console.WriteLine("¡No se pudo generar el comprobante!")
                    Console.Read()
                    Exit Sub
                End If
                Console.WriteLine("OK - CFDI Timbrado.")
                If rdrReceptor("GLOB_ADENDAAMC").ToString.Trim = "Y" And Not reciboPago Then
                    Console.WriteLine("Insertando Addenda...")
                    LlenaAddenda()
                    Console.WriteLine("OK - Addenda Insertada.")
                End If
            Else
                Console.WriteLine("¡El CFDI no se generó!")
                Console.Read()
                Exit Sub
            End If
        Catch ex As Exception
            Console.WriteLine("No se puede generar el comprobante debido a:" & vbCrLf & vbCrLf & ex.Message)
            Console.Read()
            Exit Sub
        End Try

        Dim NombreArchivo2 As String = rdrFactGral("IVNUM").ToString.Substring(0, 1) & rdrFactGral("IVNUM").ToString.Substring(1, 7)
        Dim NombreArchivo1 As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM") & "\CF-CFDI\"
        Dim NombreArchivo As String = NombreArchivo1 & rdrReceptor("VATNUM") & "_" & NombreArchivo2 & ".xml"

        Dim strQuery As String = GeneraQueryComprobante()

        Dim con As New SqlConnection(Datos.ConnectionString)
        con.Open()
        Dim trans As SqlTransaction = con.BeginTransaction()
        Dim cmd As New SqlCommand()
        cmd.Connection = con
        cmd.Transaction = trans


        Dim msXML1 As MemoryStream = New MemoryStream
        Dim writer1 As XmlTextWriter = New XmlTextWriter(msXML1, System.Text.Encoding.Unicode)
        Doc.Save(writer1)
        msXML1.Position = 0

        Try
            cmd.Parameters.Add("@XML", SqlDbType.Xml)
            cmd.Parameters.Item("@XML").Value = System.Text.Encoding.Unicode.GetString(msXML1.ToArray)
            cmd.CommandText = strQuery
            cmd.ExecuteNonQuery()
            Dim folioFiscal As String
            Dim fechaFiscal As String
            cmd.Parameters.Clear()
            If Not reciboPago Then
                folioFiscal = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).FirstChild.Attributes("UUID").Value
                fechaFiscal = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).FirstChild.Attributes("FechaTimbrado").Value
            Else
                folioFiscal = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).ChildNodes(1).Attributes("UUID").Value
                fechaFiscal = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).ChildNodes(1).Attributes("FechaTimbrado").Value
            End If
            cmd.CommandText = "UPDATE " & Empresa & ".dbo.INVOICES SET GLOB_VALIDA='Y', GLOB_FOLIOFISCAL='" & folioFiscal & "', GLOB_FECHACFDI='" & fechaFiscal & "' WHERE IV=" & rdrFactGral("IV")
            cmd.ExecuteNonQuery()

            trans.Commit()
        Catch ex As Exception
            trans.Rollback()
            Console.WriteLine("Error al generar el registro del comprobante!")
            Console.Write(ex.Message)
            Console.Read()
            Exit Sub
        End Try

        If Not System.IO.Directory.Exists(NombreArchivo1) Then
            System.IO.Directory.CreateDirectory(NombreArchivo1)
        End If

        Doc.Save(NombreArchivo)
        If reciboPago Then modificaXMLRecibo(Doc)
        'If rdrReceptor("GLOB_ADENDAAMC").ToString.Trim = "Y" Then
        '    Console.WriteLine("Cargando XML con Addenda al Servidor FTP...")
        '    'UploadFTP(NombreArchivo)
        '    FileCopy(NombreArchivo, "\\remoto\ftpcolgate\colgate\" & rdrReceptor("VATNUM") & NombreArchivo2 & ".xml")
        '    Console.WriteLine("OK - Documento Cargado.")
        'End If

        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        Doc.Save(writer)
        msXML.Position = 0
        Dim dsNuevoComp As New System.Data.DataSet
        'dsNuevoComp.ReadXmlSchema("ds_cfdv3.xsd")
        dsNuevoComp.ReadXml(msXML)
        Datos.CreaDataSetCFDI3_3(dsNuevoComp, False, CadenaOriginal_TFD, IIf(rdrFactGral("DEBIT").ToString.Trim = "D", "FACTURA", "NOTA DE CREDITO"), True)

        Try
            GeneraReporte(dsNuevoComp, NombreArchivo)
            If Not rdrReceptor("EMAIL") Is System.DBNull.Value And Not rdrReceptor("EMAIL") Is String.Empty And Not rdrReceptor("EMAIL").ToString.Trim = "" Then
                EnviaCorreo(NombreArchivo)
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.Read()
        End Try
    End Sub

    Private Sub GenerarComprobanteCFDI() 'ByRef filaGen As SqlDataReader, ByRef dtDet As SqlDataReader, ByVal rdrEmisor As SqlDataReader, ByRef Empresa As String)
        Doc = New XmlDocument

        Dim Params As New FirmaParams
        Params.ArchivoXSD = "cfdv32.xsd"
        Params.ArchivoXSLT = "cadenaoriginal_3_2.xslt"
        Params.NodoCert = "Comprobante"
        Params.EspacioNombres = "http://www.sat.gob.mx/cfd/3"
        Params.AtrCertif = "certificado"
        Params.AtrNoCert = "noCertificado"
        Params.AtrSello = "sello"

        Try
            LlenaXMLCFDI()
            Console.WriteLine("OK - CFDI creado.")
            Console.WriteLine("Firmando CFDI...")
            Datos.Firma(Params)
            Console.WriteLine("OK - CFDI firmado.")
            Console.WriteLine("Validando CFDI...")
            If Datos.Valida(Params) Then
                Console.WriteLine("OK - CFDI Validado.")
                Console.WriteLine("Timbrando CFDI...")
                If Not Datos.TimbraEDICOM() Then
                    Console.WriteLine("¡No se pudo generar el comprobante!")
                    Console.Read()
                    Exit Sub
                End If
                Console.WriteLine("OK - CFDI Timbrado.")
                If rdrReceptor("GLOB_ADENDAAMC").ToString.Trim = "Y" Then
                    Console.WriteLine("Insertando Addenda...")
                    LlenaAddenda()
                    Console.WriteLine("OK - Addenda Insertada.")
                End If
            Else
                Console.WriteLine("¡El CFDI no se generó!")
                Console.Read()
                Exit Sub
            End If
        Catch ex As Exception
            Console.WriteLine("No se puede generar el comprobante debido a:" & vbCrLf & vbCrLf & ex.Message)
            Console.Read()
            Exit Sub
        End Try

        Dim NombreArchivo2 As String = rdrFactGral("IVNUM").ToString.Substring(0, 1) & rdrFactGral("IVNUM").ToString.Substring(1, 7)
        Dim NombreArchivo1 As String = Datos.RutaDocs & "\Documentos\" & rdrEmisor("VATNUM") & "\CF-CFDI\"
        Dim NombreArchivo As String = NombreArchivo1 & rdrReceptor("VATNUM") & "_" & NombreArchivo2 & ".xml"

        Dim strQuery As String = GeneraQueryComprobante()

        Dim con As New SqlConnection(Datos.ConnectionString)
        con.Open()
        Dim trans As SqlTransaction = con.BeginTransaction()
        Dim cmd As New SqlCommand()
        cmd.Connection = con
        cmd.Transaction = trans


        Dim msXML1 As MemoryStream = New MemoryStream
        Dim writer1 As XmlTextWriter = New XmlTextWriter(msXML1, System.Text.Encoding.Unicode)
        Doc.Save(writer1)
        msXML1.Position = 0

        Try
            cmd.Parameters.Add("@XML", SqlDbType.Xml)
            cmd.Parameters.Item("@XML").Value = System.Text.Encoding.Unicode.GetString(msXML1.ToArray)
            cmd.CommandText = strQuery
            cmd.ExecuteNonQuery()

            cmd.Parameters.Clear()
            Dim folioFiscal As String = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).FirstChild.Attributes("UUID").Value
            Dim fechaFiscal As String = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).FirstChild.Attributes("FechaTimbrado").Value
            cmd.CommandText = "UPDATE " & Empresa & ".dbo.INVOICES SET GLOB_VALIDA='Y', GLOB_FOLIOFISCAL='" & folioFiscal & "', GLOB_FECHACFDI='" & fechaFiscal & "' WHERE IV=" & rdrFactGral("IV")
            cmd.ExecuteNonQuery()

            trans.Commit()
        Catch ex As Exception
            trans.Rollback()
            Console.WriteLine("Error al generar el registro del comprobante!")
            Console.Write(ex.Message)
            Console.Read()
            Exit Sub
        End Try

        If Not System.IO.Directory.Exists(NombreArchivo1) Then
            System.IO.Directory.CreateDirectory(NombreArchivo1)
        End If

        Doc.Save(NombreArchivo)

        'If rdrReceptor("GLOB_ADENDAAMC").ToString.Trim = "Y" Then
        '    Console.WriteLine("Cargando XML con Addenda al Servidor FTP...")
        '    'UploadFTP(NombreArchivo)
        '    FileCopy(NombreArchivo, "\\remoto\ftpcolgate\colgate\" & rdrReceptor("VATNUM") & NombreArchivo2 & ".xml")
        '    Console.WriteLine("OK - Documento Cargado.")
        'End If

        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        Doc.Save(writer)
        msXML.Position = 0
        Dim dsNuevoComp As New System.Data.DataSet
        'dsNuevoComp.ReadXmlSchema("ds_cfdv3.xsd")
        dsNuevoComp.ReadXml(msXML)
        Datos.CreaDataSetCFDI(dsNuevoComp, False, CadenaOriginal_TFD, IIf(rdrFactGral("DEBIT").ToString.Trim = "D", "FACTURA", "NOTA DE CREDITO"), True)

        Try
            GeneraReporte(dsNuevoComp, NombreArchivo)
            If Not rdrReceptor("EMAIL") Is System.DBNull.Value And Not rdrReceptor("EMAIL") Is String.Empty And Not rdrReceptor("EMAIL").ToString.Trim = "" Then
                EnviaCorreo(NombreArchivo)
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.Read()
        End Try
    End Sub

    Private Sub UploadFTP(ByVal rutaOrigen As String)
        Dim ArchivoArr As String() = rutaOrigen.Split("\")
        Dim Url As String = "ftp://concresamx.myvnc.com/" & ArchivoArr(UBound(ArchivoArr))
        Dim Usuario As String = "coprueba"
        Dim Password As String = "L93sh$8a"

        Try
            Dim ftp As FtpWebRequest
            ftp = CType(WebRequest.Create(New Uri(Url)), FtpWebRequest)
            ftp.Credentials = New NetworkCredential(Usuario, Password)
            ftp.KeepAlive = True
            ftp.UseBinary = True
            ftp.Method = WebRequestMethods.Ftp.UploadFile

            Dim fs = IO.File.OpenRead(rutaOrigen)
            Dim buffer(fs.Length) As Byte
            fs.Read(buffer, 0, buffer.Length)
            fs.Close()

            Dim ftpstream As Stream = ftp.GetRequestStream
            ftpstream.Write(buffer, 0, buffer.Length)
            ftpstream.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Private Function DeterminaSerie() As String()
        Dim IVNUM As String = rdrFactGral("IVNUM").ToString
        Dim Serie As String = String.Empty
        Dim Folio As String = String.Empty

        For Each letra As Char In IVNUM
            If Not IsNumeric(letra) Then
                Serie &= letra
            Else
                Folio &= letra
            End If
        Next

        Return {Serie, Folio}
    End Function

    Private Sub LlenaXMLCFDI3_3() 'ByRef Doc As XmlDocument, ByRef filaGen As SqlDataReader, ByRef dtDet As SqlDataReader, ByRef rdrEmisor As SqlDataReader)
        Dim nattr As XmlAttribute

        'Doc.Load("cfdi-base.xml")
        Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?><cfdi:Comprobante xmlns:cfdi=""http://www.sat.gob.mx/cfd/3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tfd=""http://www.sat.gob.mx/TimbreFiscalDigital"" xsi:schemaLocation=""http://www.sat.gob.mx/cfd/3 http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/TimbreFiscalDigital http://www.sat.gob.mx/sitio_internet/cfd/timbrefiscaldigital/TimbreFiscalDigitalv11.xsd"">  </cfdi:Comprobante>")
        'Doc.AppendChild(Doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty))
        '' Crea nodo comprobante y agrega sus atributos
        'Dim Comprobante As XmlNode = Doc.CreateElement("cfdi", "Comprobante", "http://www.sat.gob.mx/cfd/3")
        Dim Comprobante As XmlNode = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0)
        'Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Fecha")
        nattr.Value = Format(Now(), "yyyy-MM-ddTHH:mm:ss")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("MetodoPago")
        nattr.Value = rdrFactGral("GLFO_METODOP")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("TipoDeComprobante")
        nattr.Value = IIf(rdrFactGral("DEBIT").ToString.Trim = "D", "I", "E")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("FormaPago")
        nattr.Value = rdrFactGral("GLOB_FORMAPAGO").ToString.Substring(0, 2)
        Comprobante.Attributes.Append(nattr)
        If Not rdrReceptor("PAYDES") Is System.DBNull.Value And Not rdrReceptor("PAYDES") Is String.Empty And Not rdrReceptor("PAYDES").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("CondicionesDePago")
            nattr.Value = rdrReceptor("PAYDES").ToString
            Comprobante.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("Version")
        nattr.Value = "3.3"
        Comprobante.Attributes.Append(nattr)
        If Replace(Replace(rdrFactGral("DISCOUNT").ToString, "$", ""), ",", "") > 0 Then
            nattr = Doc.CreateAttribute("Descuento")
            nattr.Value = Replace(Replace(rdrFactGral("DISCOUNT").ToString, "$", ""), ",", "")
            Comprobante.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("SubTotal")
        nattr.Value = Math.Round(CDbl(Replace(rdrFactGral("DISPRICE"), "$", "")), 2)
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("TipoCambio")
        nattr.Value = Replace(Replace(rdrFactGral("EXCHANGE").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Moneda")
        nattr.Value = Replace(Replace(rdrFactGral("CODE").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Total")
        nattr.Value = Replace(Replace(rdrFactGral("AFTERWTAX").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)

        Dim arrSerie As String() = DeterminaSerie()
        If Not arrSerie(0) Is String.Empty Then
            nattr = Doc.CreateAttribute("Serie")
            nattr.Value = arrSerie(0).ToUpper
            Comprobante.Attributes.Append(nattr)
        End If
        If Not arrSerie(1) Is String.Empty Then
            nattr = Doc.CreateAttribute("Folio")
            nattr.Value = arrSerie(1)
            Comprobante.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("LugarExpedicion")
        nattr.Value = rdrEmisor("ZIP")
        Comprobante.Attributes.Append(nattr)

        'Crea nodo CFDIRelacionado si es necesario
        If rdrFactGral("GLFO_SUSTITUYE") = "Y" Then
            Dim CfdiRelacionados As XmlNode = Doc.CreateElement("cfdi", "CfdiRelacionados", "http://www.sat.gob.mx/cfd/3")
            AppendAttributeXML(CfdiRelacionados, Doc.CreateAttribute("TipoRelacion"), rdrFactGral("GLFO_TRELACION"))
            Dim CfdiRelacionado As XmlNode = Doc.CreateElement("cfdi", "CfdiRelacionado", "http://www.sat.gob.mx/cfd/3")
            AppendAttributeXML(CfdiRelacionado, Doc.CreateAttribute("UUID"), rdrFactRel("GLOB_FOLIOFISCAL"))
            CfdiRelacionados.AppendChild(CfdiRelacionado)
            Comprobante.AppendChild(CfdiRelacionados)
        End If

        ' Crea nodo emisor y agrega sus atributos
        Dim Emisor As XmlNode = Doc.CreateElement("cfdi", "Emisor", "http://www.sat.gob.mx/cfd/3")
        'Dim fila As System.Data.DataRow = ds.Tables("Emisor").Rows(0)
        nattr = Doc.CreateAttribute("Rfc")
        nattr.Value = rdrEmisor("VATNUM")
        Emisor.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Nombre")
        nattr.Value = rdrEmisor("COMPDES")
        Emisor.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("RegimenFiscal")
        nattr.Value = rdrEmisor("GLFO_REGIMEN")
        Emisor.Attributes.Append(nattr)

        ' Agrega el nodo Emisor dentro del nodo Comprobante
        Comprobante.AppendChild(Emisor)

        ' Crea nodo receptor y agrega sus atributos
        Dim Receptor As XmlNode = Doc.CreateElement("cfdi", "Receptor", "http://www.sat.gob.mx/cfd/3")
        'Dim fila1 As System.Data.DataRowView = bsReceptores.Current
        If Not rdrReceptor("VATNUM") Is System.DBNull.Value And Not rdrReceptor("VATNUM") Is String.Empty And Not rdrReceptor("VATNUM").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("Rfc")
            nattr.Value = Replace(Replace(rdrReceptor("VATNUM").ToString, "-", ""), " ", "").Normalize
            Receptor.Attributes.Append(nattr)
        Else
            nattr = Doc.CreateAttribute("Rfc")
            nattr.Value = IIf(rdrFactGral("CODE").ToString = "MXP", "XAXX010101000", "XEXX010101000")
            Receptor.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("Nombre")
        nattr.Value = rdrReceptor("CUSTDES")
        Receptor.Attributes.Append(nattr)

        If Not rdrReceptor("COUNTRYNAME") Is System.DBNull.Value And Not rdrReceptor("COUNTRYNAME") Is String.Empty And Not rdrReceptor("COUNTRYNAME").ToString.Trim = "" Then
            If Not rdrReceptor("COUNTRYNAME") = "MEXICO" Then
                nattr = Doc.CreateAttribute("ResidenciaFiscal")
                nattr.Value = rdrReceptor("COUNTRYCODE")
                Receptor.Attributes.Append(nattr)
            End If
        Else
            If Not rdrFactGral("CODE").ToString Is "MXP" Then
                nattr = Doc.CreateAttribute("ResidenciaFiscal")
                nattr.Value = IIf(rdrFactGral("CODE").ToString = "MXP", "MEX", "USA")
                Receptor.Attributes.Append(nattr)
            End If
        End If

        nattr = Doc.CreateAttribute("UsoCFDI")
        nattr.Value = rdrFactGral("GLFO_USOCFDI")
        Receptor.Attributes.Append(nattr)

        ' Agrega el nodo Receptor dentro del nodo Comprobante
        Comprobante.AppendChild(Receptor)

        ' Crea nodo Conceptos
        Dim Conceptos As XmlNode = Doc.CreateElement("cfdi", "Conceptos", "http://www.sat.gob.mx/cfd/3")
        Dim totalTraslado As Double = 0
        ' Por cada concepto en la tabla crea un nodo Concepto y sus atributos correspondientes y los subnodos InformacionAduanera
        'Dim suma As Double = 0
        'Console.WriteLine(rdrFactItems.Rows.Count)
        Dim totalRetenciones As Double = 0
        For Each Item As DataRow In rdrFactItems.Rows

            ' Crea nodo Concepto y sus atriibutos
            Dim Concepto As XmlNode = Doc.CreateElement("cfdi", "Concepto", "http://www.sat.gob.mx/cfd/3")
            nattr = Doc.CreateAttribute("Cantidad")
            nattr.Value = IIf(Item("QUANT") Is System.DBNull.Value, 0, Math.Round(CDbl(Item("QUANT").ToString.Trim), 4))
            Concepto.Attributes.Append(nattr)

            If Not Item("PARTNAME") Is System.DBNull.Value And Not Item("PARTNAME").ToString Is String.Empty Then
                nattr = Doc.CreateAttribute("NoIdentificacion")
                nattr.Value = Item("PARTNAME")
                Concepto.Attributes.Append(nattr)
            End If

            nattr = Doc.CreateAttribute("ClaveUnidad")
            nattr.Value = Item("UNITNAME")
            Concepto.Attributes.Append(nattr)

            nattr = Doc.CreateAttribute("Unidad")
            nattr.Value = Item("UNITDES")
            Concepto.Attributes.Append(nattr)

            Dim observacionesItem As String = ""
            If Not String.IsNullOrEmpty(Item("OBSERVACIONES").ToString) Then
                observacionesItem = Observaciones(Item("OBSERVACIONES").ToString)
            End If

            nattr = Doc.CreateAttribute("Descripcion")
            nattr.Value = IIf(String.IsNullOrEmpty(Item("NONSTANDARD").ToString), Item("PARTDES"), Item("PARTDES2")) & observacionesItem
            Concepto.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("ValorUnitario")
            nattr.Value = IIf(Item("PRICE") Is System.DBNull.Value, 0, Replace(Item("PRICE"), "$", ""))
            Concepto.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("Importe")
            nattr.Value = IIf(Item("QPRICE") Is System.DBNull.Value, 0, Replace(Item("QPRICE"), "$", ""))
            'suma += nattr.Value
            Concepto.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("ClaveProdServ")
            nattr.Value = Item("GLFO_CLAVEP")
            Concepto.Attributes.Append(nattr)

            Dim ImpuestosPart As XmlNode = Doc.CreateElement("cfdi", "Impuestos", "http://www.sat.gob.mx/cfd/3")
            Dim TrasladosPart As XmlNode = Doc.CreateElement("cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3")
            Dim RetencionesPart As XmlNode = Doc.CreateElement("cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/3")
            Dim base As Double = IIf(Item("QPRICE") Is System.DBNull.Value, 0, Replace(Item("QPRICE"), "$", ""))
            If Item("IVTAX") > 0 Then
                Dim impor = Math.Round(base * 0.16, 6)
                Dim TrasladoP As XmlNode = Doc.CreateElement("cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3")
                AgregaImpuesto(Doc, TrasladoP, base, "002", "Tasa", "0.16", FormatNumber(impor, 6, , , Microsoft.VisualBasic.TriState.False))
                TrasladosPart.AppendChild(TrasladoP)
                ImpuestosPart.AppendChild(TrasladosPart)
                Concepto.AppendChild(ImpuestosPart)
                totalTraslado += impor
                Tras = True
            End If

            If Not String.IsNullOrEmpty(Item("TAXPERCENT")) And Item("TAXPERCENT") > 0 Then
                Dim tasa As Double = Math.Round(Item("TAXPERCENT") / 100, 6)
                Dim impor As Double = Math.Round(base * tasa, 2)
                Dim RetenidoP As XmlNode = Doc.CreateElement("cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3")
                AgregaImpuesto(Doc, RetenidoP, base, "002", "Tasa", tasa, FormatNumber(impor, 2, , , Microsoft.VisualBasic.TriState.False))
                RetencionesPart.AppendChild(RetenidoP)
                ImpuestosPart.AppendChild(RetencionesPart)
                Concepto.AppendChild(ImpuestosPart)
                totalRetenciones += impor
                Retencion = True
            End If
            ' Crea nodo CuentaPredial
            If Not rdrFactGral("GLFO_OTRASFE") Is System.DBNull.Value And Not rdrFactGral("GLFO_OTRASFE") Is String.Empty And Not rdrFactGral("GLFO_OTRASFE").ToString.Trim = "" Then
                Dim CuentaPredial As XmlNode = Doc.CreateElement("cfdi", "CuentaPredial", "http://www.sat.gob.mx/cfd/3")
                nattr = Doc.CreateAttribute("Numero")
                nattr.Value = rdrFactGral("GLFO_OTRASFE")
                CuentaPredial.Attributes.Append(nattr)

                ' Agrega el nodo CuentaPredial dentro del nodo Concepto
                Concepto.AppendChild(CuentaPredial)
            End If
            Conceptos.AppendChild(Concepto)
        Next


        ' Agrega el nodo Conceptos dentro del nodo Comprobante
        Comprobante.AppendChild(Conceptos)

        ' Crea nodo Impuestos y sus atributos
        Dim Impuestos As XmlNode = Doc.CreateElement("cfdi", "Impuestos", "http://www.sat.gob.mx/cfd/3")
        'If chkIVAtras.Checked Or chkIVAret.Checked Or chkISRret.Checked Or chkFleteRet.Checked Then
        'Dim totalRetenciones As Double = 0
        Dim nattr1 As XmlAttribute



        If Not rdrFactGral("WTAX") Is System.DBNull.Value And Not rdrFactGral("WTAX") Is String.Empty And Not rdrFactGral("WTAX").ToString.Trim = "" And Not CDbl(rdrFactGral("WTAX").ToString.Trim) = 0 Then
            ' Crea nodo Retenciones por si se tiene que utilizar
            Dim Retenciones As XmlNode = Doc.CreateElement("cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/3")

            ' Crea nodo Retencion y sus atributos
            Dim Retencion As XmlNode = Doc.CreateElement("cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3")
            nattr1 = Doc.CreateAttribute("Impuesto")
            nattr1.Value = "002"
            Retencion.Attributes.Append(nattr1)
            nattr1 = Doc.CreateAttribute("Importe")
            nattr1.Value = Replace(Replace(rdrFactGral("WTAX"), "$", ""), ",", "")
            Retencion.Attributes.Append(nattr1)

            ' Agrega el nodo Retencion dentro del nodo Retenciones
            Retenciones.AppendChild(Retencion)

            nattr = Doc.CreateAttribute("TotalImpuestosRetenidos")
            nattr.Value = Replace(Replace(rdrFactGral("WTAX"), "$", ""), ",", "")
            Impuestos.Attributes.Append(nattr)

            ' Agrega el nodo Retenciones dentro del nodo Impuestos
            Impuestos.AppendChild(Retenciones)
            Comprobante.AppendChild(Impuestos)
        End If

        If totalTraslado Then
            nattr = Doc.CreateAttribute("TotalImpuestosTrasladados")
            nattr.Value = FormatNumber(totalTraslado, 2, , , Microsoft.VisualBasic.TriState.False)
            Impuestos.Attributes.Append(nattr)

            ' Crea nodo Traslados
            Dim Traslados As XmlNode = Doc.CreateElement("cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3")
            ' Crea nodo Traslado y sus atributos
            Dim Traslado As XmlNode = Doc.CreateElement("cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3")
            nattr1 = Doc.CreateAttribute("Impuesto")
            nattr1.Value = "002"
            Traslado.Attributes.Append(nattr1)
            nattr1 = Doc.CreateAttribute("TipoFactor")
            nattr1.Value = "Tasa"
            Traslado.Attributes.Append(nattr1)
            nattr1 = Doc.CreateAttribute("TasaOCuota")
            nattr1.Value = FormatNumber((CDbl(rdrFactGral("TAXPERCENT")) / 100), 6)
            Traslado.Attributes.Append(nattr1)
            nattr1 = Doc.CreateAttribute("Importe")
            nattr1.Value = FormatNumber(totalTraslado, 6, , , Microsoft.VisualBasic.TriState.False)
            Traslado.Attributes.Append(nattr1)

            'TotalFactura = Math.Round(CDbl(Replace(rdrFactGral("DISPRICE"), "$", "")), 2) + Math.Round(totalTraslado, 2)
            TotalFactura = Comprobante.Attributes("Total").Value
            ' Agrega el nodo Traslado dentro del nodo Traslados
            Traslados.AppendChild(Traslado)

            ' Agrega el nodo Traslados dentro del nodo Impuestos
            Impuestos.AppendChild(Traslados)
            'End If

            ' Agrega el nodo Impuestos dentro del nodo Comprobante
            Comprobante.AppendChild(Impuestos)
        End If

        Dim complemento As XmlNode = Doc.CreateElement("cfdi", "Complemento", "http://www.sat.gob.mx/cfd/3")
        Comprobante.AppendChild(complemento)

        Doc.AppendChild(Comprobante)
        Doc.Save("cfdi.xml")

        'Process.Start("explorer.exe", "cfdi.xml")
        'Console.Read()
    End Sub

    Private Sub LlenaXMLCFDIRecibo3_3() 'ByRef Doc As XmlDocument, ByRef filaGen As SqlDataReader, ByRef dtDet As SqlDataReader, ByRef rdrEmisor As SqlDataReader)
        Dim nattr As XmlAttribute

        'Doc.Load("cfdi-base.xml")
        Doc.LoadXml("<?xml version=""1.0"" encoding=""UTF-8""?><cfdi:Comprobante xmlns:cfdi=""http://www.sat.gob.mx/cfd/3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:pago10=""http://www.sat.gob.mx/Pagos"" xmlns:tfd=""http://www.sat.gob.mx/TimbreFiscalDigital"" xsi:schemaLocation=""http://www.sat.gob.mx/cfd/3 http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/Pagos http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos10.xsd http://www.sat.gob.mx/TimbreFiscalDigital http://www.sat.gob.mx/sitio_internet/cfd/timbrefiscaldigital/TimbreFiscalDigitalv11.xsd"">  </cfdi:Comprobante>")
        'Doc.AppendChild(Doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty))
        '' Crea nodo comprobante y agrega sus atributos
        'Dim Comprobante As XmlNode = Doc.CreateElement("cfdi", "Comprobante", "http://www.sat.gob.mx/cfd/3")
        Dim Comprobante As XmlNode = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0)
        'Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Fecha")
        nattr.Value = Format(Now(), "yyyy-MM-ddTHH:mm:ss")
        Comprobante.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("TipoDeComprobante")
        nattr.Value = "P"
        Comprobante.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("Version")
        nattr.Value = "3.3"
        Comprobante.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("SubTotal")
        nattr.Value = 0
        Comprobante.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("Moneda")
        nattr.Value = "XXX"
        Comprobante.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("Total")
        nattr.Value = 0
        Comprobante.Attributes.Append(nattr)

        Dim arrSerie As String() = DeterminaSerie()
        If Not arrSerie(0) Is String.Empty Then
            nattr = Doc.CreateAttribute("Serie")
            nattr.Value = arrSerie(0).ToUpper
            Comprobante.Attributes.Append(nattr)
        End If
        If Not arrSerie(1) Is String.Empty Then
            nattr = Doc.CreateAttribute("Folio")
            nattr.Value = arrSerie(1)
            Comprobante.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("LugarExpedicion")
        nattr.Value = rdrEmisor("ZIP")
        Comprobante.Attributes.Append(nattr)

        'Crea nodo CFDIRelacionado si es necesario
        'If rdrFactGral("GLFO_SUSTITUYE") = "Y" Then
        '    Dim CfdiRelacionados As XmlNode = Doc.CreateElement("cfdi", "CfdiRelacionados", "http://www.sat.gob.mx/cfd/3")
        '    AppendAttributeXML(CfdiRelacionados, Doc.CreateAttribute("TipoRelacion"), rdrFactGral("GLFO_TRELACION"))
        '    Dim CfdiRelacionado As XmlNode = Doc.CreateElement("cfdi", "CfdiRelacionado", "http://www.sat.gob.mx/cfd/3")
        '    AppendAttributeXML(CfdiRelacionado, Doc.CreateAttribute("UUID"), rdrFactRel("GLOB_FOLIOFISCAL"))
        '    CfdiRelacionados.AppendChild(CfdiRelacionado)
        '    Comprobante.AppendChild(CfdiRelacionados)
        'End If

        ' Crea nodo emisor y agrega sus atributos
        Dim Emisor As XmlNode = Doc.CreateElement("cfdi", "Emisor", "http://www.sat.gob.mx/cfd/3")
        'Dim fila As System.Data.DataRow = ds.Tables("Emisor").Rows(0)
        nattr = Doc.CreateAttribute("Rfc")
        nattr.Value = rdrEmisor("VATNUM")
        Emisor.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Nombre")
        nattr.Value = rdrEmisor("COMPDES")
        Emisor.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("RegimenFiscal")
        nattr.Value = rdrEmisor("GLFO_REGIMEN")
        Emisor.Attributes.Append(nattr)

        ' Agrega el nodo Emisor dentro del nodo Comprobante
        Comprobante.AppendChild(Emisor)

        ' Crea nodo receptor y agrega sus atributos
        Dim Receptor As XmlNode = Doc.CreateElement("cfdi", "Receptor", "http://www.sat.gob.mx/cfd/3")
        If rdrFactGral("GLFO_FACTORAJE") = "Y" Then
            nattr = Doc.CreateAttribute("Rfc")
            nattr.Value = rdrFactGral("RFC")
            Receptor.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("Nombre")
            nattr.Value = rdrFactGral("Nombre")
            Receptor.Attributes.Append(nattr)
        Else
            'Dim fila1 As System.Data.DataRowView = bsReceptores.Current
            If Not rdrReceptor("VATNUM") Is System.DBNull.Value And Not rdrReceptor("VATNUM") Is String.Empty And Not rdrReceptor("VATNUM").ToString.Trim = "" Then
                nattr = Doc.CreateAttribute("Rfc")
                nattr.Value = Replace(Replace(rdrReceptor("VATNUM").ToString, "-", ""), " ", "").Normalize
                Receptor.Attributes.Append(nattr)
            Else
                nattr = Doc.CreateAttribute("Rfc")
                nattr.Value = IIf(rdrFactGral("CODE").ToString = "MXP", "XAXX010101000", "XEXX010101000")
                Receptor.Attributes.Append(nattr)
            End If
            nattr = Doc.CreateAttribute("Nombre")
            nattr.Value = rdrReceptor("CUSTDES")
            Receptor.Attributes.Append(nattr)

            If Not rdrReceptor("COUNTRYNAME") Is System.DBNull.Value And Not rdrReceptor("COUNTRYNAME") Is String.Empty And Not rdrReceptor("COUNTRYNAME").ToString.Trim = "" Then
                If Not rdrReceptor("COUNTRYNAME") = "MEXICO" Then
                    nattr = Doc.CreateAttribute("ResidenciaFiscal")
                    nattr.Value = rdrReceptor("COUNTRYCODE")
                    Receptor.Attributes.Append(nattr)
                End If
            Else
                If Not rdrFactGral("CODE").ToString Is "MXP" Then
                    nattr = Doc.CreateAttribute("ResidenciaFiscal")
                    nattr.Value = IIf(rdrFactGral("CODE").ToString = "MXP", "MEX", "USA")
                    Receptor.Attributes.Append(nattr)
                End If
            End If
        End If
        nattr = Doc.CreateAttribute("UsoCFDI")
        nattr.Value = "P01"
        Receptor.Attributes.Append(nattr)

        ' Agrega el nodo Receptor dentro del nodo Comprobante
        Comprobante.AppendChild(Receptor)


        ' Crea nodo Conceptos
        Dim Conceptos As XmlNode = Doc.CreateElement("cfdi", "Conceptos", "http://www.sat.gob.mx/cfd/3")

        ' Crea nodo Concepto y sus atriibutos
        Dim Concepto As XmlNode = Doc.CreateElement("cfdi", "Concepto", "http://www.sat.gob.mx/cfd/3")
        nattr = Doc.CreateAttribute("Cantidad")
        nattr.Value = 1
        Concepto.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("ClaveUnidad")
        nattr.Value = "ACT"
        Concepto.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("Descripcion")
        nattr.Value = "Pago"
        Concepto.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("ValorUnitario")
        nattr.Value = 0
        Concepto.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("Importe")
        nattr.Value = 0
        Concepto.Attributes.Append(nattr)

        nattr = Doc.CreateAttribute("ClaveProdServ")
        nattr.Value = "84111506"
        Concepto.Attributes.Append(nattr)

        'If (Math.Round(Item("QUANT") * Item("PRICE"), 2) - CDbl(Item("QPRICE"))) > 0 Then
        '    nattr = Doc.CreateAttribute("Descuento")
        '    nattr.Value = CDbl(Item("QUANT")) * CDbl(Item("PRICE")) - CDbl(Item("QPRICE"))
        '    Concepto.Attributes.Append(nattr)
        '    Descuento += CDbl(Item("QUANT")) * CDbl(Item("PRICE")) - CDbl(Item("QPRICE"))
        'End If
        Conceptos.AppendChild(Concepto)

        ' Agrega el nodo Conceptos dentro del nodo Comprobante
        Comprobante.AppendChild(Conceptos)

        Dim complemento As XmlNode = Doc.CreateElement("cfdi", "Complemento", "http://www.sat.gob.mx/cfd/3")

        Dim Pagos As XmlNode = Doc.CreateElement("pago10", "Pagos", "http://www.sat.gob.mx/Pagos")
        AppendAttributeXML(Pagos, Doc.CreateAttribute("Version"), "1.0")

        Dim Pago As XmlNode = Doc.CreateElement("pago10", "Pago", "http://www.sat.gob.mx/Pagos")
        AppendAttributeXML(Pago, Doc.CreateAttribute("FechaPago"), Format(Convert.ToDateTime(rdrFactGral("GLFO_FECPAGO").ToString), "yyyy-MM-ddTHH:mm:ss"))
        AppendAttributeXML(Pago, Doc.CreateAttribute("FormaDePagoP"), rdrFactGral("GLOB_FORMAPAGO").ToString.Substring(0, 2))
        AppendAttributeXML(Pago, Doc.CreateAttribute("MonedaP"), Replace(Replace(rdrFactGral("CODE").ToString, "$", ""), ",", ""))
        If Replace(Replace(rdrFactGral("CODE").ToString, "$", ""), ",", "") <> "MXN" Then
            AppendAttributeXML(Pago, Doc.CreateAttribute("TipoCambioP"), Replace(Replace(rdrFactGral("EXCHANGE").ToString, "$", ""), ",", ""))
        End If
        If Not Replace(Replace(rdrFactGral("CODE").ToString, "$", ""), ",", "") Is "MXN" Then
            'AppendAttributeXML(Pago, Doc.CreateAttribute("TipoCambioP"), tbTipoCambio.Text)
        End If
        If rdrFactGral("GLOB_FORMAPAGO").ToString.Substring(0, 2) <> "01" And rdrFactGral("GLOB_FORMAPAGO").ToString.Substring(0, 2) <> "17" Then
            'AppendAttributeXML(Pago, Doc.CreateAttribute("Monto"), FormatNumber(Replace(Replace(rdrFactGral("AFTERWTAX").ToString, "$", ""), ",", ""), 2, , , Tristate.TristateFalse))
            AppendAttributeXML(Pago, Doc.CreateAttribute("RfcEmisorCtaBen"), rdrBanco("BIC"))
            AppendAttributeXML(Pago, Doc.CreateAttribute("CtaBeneficiario"), rdrBanco("PAYACCOUNT"))
        End If
        'monto acumulado
        Dim monto As Double = 0.0
        Dim totalRecibo As Double = 0
        'Verificamos si hay notas de credito
        Dim credito(99) As Double
        For Each factura As DataRow In rdrFactPagadas.Rows
            If factura("CREDIT1") < 0 Then
                credito(factura("GLFO_RELAC")) += factura("CREDIT1") * -1
            Else
                totalRecibo += factura("CREDIT2")
            End If
        Next
        Dim tipoCambio As Double = Math.Round(rdrFactGral("AFTERWTAX").ToString / totalRecibo, 2)

        'Creamos nodos Documento Relacionado por cada CFDI relacionado con este pago
        For Each CFDIRel As DataRow In rdrFactPagadas.Rows
            If CFDIRel("CREDIT1") > 0 Then
                'Console.WriteLine(CFDIRel("TOTPRICE"))
                'Console.Read()
                If Not String.IsNullOrEmpty(CFDIRel("IVNUM").ToString) Then
                    Dim IVNUM As String = CFDIRel("IVNUM").ToString
                    Dim Serie As String = String.Empty
                    Dim Folio As String = String.Empty

                    For Each letra As Char In IVNUM
                        If Not IsNumeric(letra) Then
                            Serie &= letra
                        Else
                            Folio &= letra
                        End If
                    Next

                    Dim DoctoRelacionado As XmlNode = Doc.CreateElement("pago10", "DoctoRelacionado", "http://www.sat.gob.mx/Pagos")
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("IdDocumento"), CFDIRel("GLOB_FOLIOFISCAL").ToString)
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("Serie"), Serie)
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("Folio"), Folio)
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("MonedaDR"), CFDIRel("CODE"))
                    'Dim tipoCambio As Double = 1
                    Dim importePagado As Double = 0
                    If Not rdrFactGral("CODE").ToString = CFDIRel("CODE") Then
                        'AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("TipoCambioDR"), CFDIRel("EXCHANGE"))
                        'tipoCambio = Math.Round(CFDIRel("IVBALANCE2") / CFDIRel("IVBALANCE"), 4)
                        AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("TipoCambioDR"), tipoCambio)
                        importePagado = Math.Round(CFDIRel("CREDIT1") / tipoCambio, 2)
                        If (CFDIRel("GLFO_RELAC") <> 0) Then importePagado -= Math.Round(credito(CFDIRel("GLFO_RELAC")) / tipoCambio, 2)
                        monto += importePagado * tipoCambio
                        'monto += CFDIRel("CREDIT1")
                    Else
                        If CFDIRel("CODE") = "MXN" Then
                            importePagado = CFDIRel("CREDIT1")
                        Else
                            importePagado = CFDIRel("CREDIT2")
                        End If
                        If (CFDIRel("GLFO_RELAC") <> 0) Then importePagado -= credito(CFDIRel("GLFO_RELAC"))
                        monto += importePagado
                    End If
                    Dim parcialidad = CFDIRel("PARCIALIDAD").ToString
                    If parcialidad = 0 Then parcialidad = 1
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("MetodoDePagoDR"), "PPD")
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("NumParcialidad"), parcialidad)
                    'Dim saldoInsoluto As Double = Math.Round((CFDIRel("IVBALANCE2") / tipoCambio) * -1, 2)
                    'Dim saldoAnterior As Double = saldoInsoluto + importePagado
                    Dim saldoAnterior As Double = (CFDIRel("IVBALANCE") * -1)
                    If (CFDIRel("GLFO_RELAC") <> 0) Then saldoAnterior -= credito(CFDIRel("GLFO_RELAC"))
                    Dim saldoInsoluto As Double = saldoAnterior - importePagado
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("ImpSaldoAnt"), FormatNumber(Math.Round(saldoAnterior, 2), 2, , , Tristate.TristateFalse))
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("ImpSaldoInsoluto"), FormatNumber(Math.Round(saldoInsoluto, 2), 2, , , Tristate.TristateFalse))
                    If rdrFactGral("CODE").ToString = CFDIRel("CODE") Then tipoCambio = CFDIRel("EXCHANGE")
                    AppendAttributeXML(DoctoRelacionado, Doc.CreateAttribute("ImpPagado"), FormatNumber(importePagado, 2, , , Tristate.TristateFalse))
                    Pago.AppendChild(DoctoRelacionado)
                End If
            End If
        Next
        Pagos.AppendChild(Pago)
        complemento.AppendChild(Pagos)
        Comprobante.AppendChild(complemento)
        AppendAttributeXML(Pago, Doc.CreateAttribute("Monto"), FormatNumber(monto, 2, , , Tristate.TristateFalse))
        Doc.AppendChild(Comprobante)
        'Doc.Save("cfdi.xml")
        'Process.Start("explorer.exe", "cfdi.xml")
    End Sub

    Private Sub LlenaXMLCFDI() 'ByRef Doc As XmlDocument, ByRef filaGen As SqlDataReader, ByRef dtDet As SqlDataReader, ByRef rdrEmisor As SqlDataReader)
        Dim nattr As XmlAttribute

        Doc.Load("cfdi-base.xml")

        'Doc.AppendChild(Doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty))
        '' Crea nodo comprobante y agrega sus atributos
        'Dim Comprobante As XmlNode = Doc.CreateElement("cfdi", "Comprobante", "http://www.sat.gob.mx/cfd/3")
        Dim Comprobante As XmlNode = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0)
        'Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("fecha")
        nattr.Value = Format(Now(), "yyyy-MM-ddTHH:mm:ss")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("formaDePago")
        nattr.Value = "PAGO EN UNA SOLA EXHIBICION"
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("tipoDeComprobante")
        nattr.Value = IIf(rdrFactGral("DEBIT").ToString.Trim = "D", "ingreso", "egreso")
        Comprobante.Attributes.Append(nattr)
        If Not rdrReceptor("GLFO_CONDPAGO") Is System.DBNull.Value And Not rdrReceptor("GLFO_CONDPAGO") Is String.Empty And Not rdrReceptor("GLFO_CONDPAGO").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("metodoDePago")
            nattr.Value = rdrReceptor("GLFO_CONDPAGO").ToString.Substring(0, 2)
            Comprobante.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("PAYDES") Is System.DBNull.Value And Not rdrReceptor("PAYDES") Is String.Empty And Not rdrReceptor("PAYDES").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("condicionesDePago")
            nattr.Value = rdrReceptor("PAYDES").ToString
            Comprobante.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("version")
        nattr.Value = "3.2"
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("descuento")
        nattr.Value = Replace(Replace(rdrFactGral("DISCOUNT").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("subTotal")
        nattr.Value = Math.Round(CDbl(Replace(rdrFactGral("DISPRICE"), "$", "")), 2)
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("TipoCambio")
        nattr.Value = Replace(Replace(rdrFactGral("EXCHANGE").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("Moneda")
        nattr.Value = Replace(Replace(rdrFactGral("CODE").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("total")
        nattr.Value = Replace(Replace(rdrFactGral("AFTERWTAX").ToString, "$", ""), ",", "")
        Comprobante.Attributes.Append(nattr)

        Dim arrSerie As String() = DeterminaSerie()
        If Not arrSerie(0) Is String.Empty Then
            nattr = Doc.CreateAttribute("serie")
            nattr.Value = arrSerie(0).ToUpper
            Comprobante.Attributes.Append(nattr)
        End If
        If Not arrSerie(1) Is String.Empty Then
            nattr = Doc.CreateAttribute("folio")
            nattr.Value = arrSerie(1)
            Comprobante.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("GLFO_NOCUENTA") Is System.DBNull.Value And Not rdrReceptor("GLFO_NOCUENTA") Is String.Empty And Not rdrReceptor("GLFO_NOCUENTA").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("NumCtaPago")
            nattr.Value = rdrReceptor("GLFO_NOCUENTA").ToString
            Comprobante.Attributes.Append(nattr)
        End If

        ' Crea nodo emisor y agrega sus atributos
        Dim Emisor As XmlNode = Doc.CreateElement("cfdi", "Emisor", "http://www.sat.gob.mx/cfd/3")
        'Dim fila As System.Data.DataRow = ds.Tables("Emisor").Rows(0)
        nattr = Doc.CreateAttribute("rfc")
        nattr.Value = rdrEmisor("VATNUM")
        Emisor.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("nombre")
        nattr.Value = rdrEmisor("COMPDES")
        Emisor.Attributes.Append(nattr)

        ' Crea nodo domicilio fiscal y agrega sus atributos
        Dim DomicilioFiscal As XmlNode = Doc.CreateElement("cfdi", "DomicilioFiscal", "http://www.sat.gob.mx/cfd/3")
        If Not rdrEmisor("ADDRESS") Is System.DBNull.Value And Not rdrEmisor("ADDRESS") Is String.Empty And Not rdrEmisor("ADDRESS").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("calle")
            nattr.Value = rdrEmisor("ADDRESS")
            DomicilioFiscal.Attributes.Append(nattr)
        End If
        If Not rdrEmisor("GLFO_NUMERO") Is System.DBNull.Value And Not rdrEmisor("GLFO_NUMERO") Is String.Empty And Not rdrEmisor("GLFO_NUMERO").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("noExterior")
            nattr.Value = rdrEmisor("GLFO_NUMERO")
            DomicilioFiscal.Attributes.Append(nattr)
        End If
        If Not rdrEmisor("GLFO_MUNICIPIO") Is System.DBNull.Value And Not rdrEmisor("GLFO_MUNICIPIO") Is String.Empty And Not rdrEmisor("GLFO_MUNICIPIO").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("noInterior")
            nattr.Value = rdrEmisor("GLFO_MUNICIPIO")
            DomicilioFiscal.Attributes.Append(nattr)
        End If
        If Not rdrEmisor("GLFO_COLONIA") Is System.DBNull.Value And Not rdrEmisor("GLFO_COLONIA") Is String.Empty And Not rdrEmisor("GLFO_COLONIA").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("colonia")
            nattr.Value = rdrEmisor("GLFO_COLONIA")
            DomicilioFiscal.Attributes.Append(nattr)
        End If

        Dim nattrLE As XmlAttribute
        ' Atributo LugarExpedicion del comprobante
        nattrLE = Doc.CreateAttribute("LugarExpedicion")

        If Not rdrEmisor("GLFO_DELEG") Is System.DBNull.Value And Not rdrEmisor("GLFO_DELEG") Is String.Empty And Not rdrEmisor("GLFO_DELEG").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("municipio")
            nattr.Value = rdrEmisor("GLFO_DELEG")
            DomicilioFiscal.Attributes.Append(nattr)

            nattrLE.Value &= rdrEmisor("GLFO_DELEG") & ", "
        End If
        If Not rdrEmisor("STATENAME") Is System.DBNull.Value And Not rdrEmisor("STATENAME") Is String.Empty And Not rdrEmisor("STATENAME").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("estado")
            nattr.Value = rdrEmisor("STATENAME")
            DomicilioFiscal.Attributes.Append(nattr)

            nattrLE.Value &= rdrEmisor("STATENAME") & ", "
        End If

        nattrLE.Value &= rdrEmisor("COUNTRYNAME")
        Comprobante.Attributes.Append(nattrLE)

        'If Not fila("Pais") Is System.DBNull.Value And Not fila("Pais") Is String.Empty And Not fila("No_Exterior") = "" Then
        nattr = Doc.CreateAttribute("pais")
        nattr.Value = rdrEmisor("COUNTRYNAME")
        DomicilioFiscal.Attributes.Append(nattr)
        'End If
        If Not rdrEmisor("ZIP") Is System.DBNull.Value And Not rdrEmisor("ZIP") Is String.Empty And Not rdrEmisor("ZIP").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("codigoPostal")
            nattr.Value = rdrEmisor("ZIP")
            DomicilioFiscal.Attributes.Append(nattr)
        End If

        ' Agrega el nodo DomicilioFiscal dentro del nodo Emisor
        Emisor.AppendChild(DomicilioFiscal)

        ' Crea nodo ExpedidoEn y agrega sus atributos
        Dim ExpedidoEn As XmlNode = Doc.CreateElement("cfdi", "ExpedidoEn", "http://www.sat.gob.mx/cfd/3")
        If Not rdrFactGral("ADDRESS") Is System.DBNull.Value And Not rdrFactGral("ADDRESS") Is String.Empty And Not rdrFactGral("ADDRESS").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("calle")
            nattr.Value = rdrFactGral("ADDRESS")
            ExpedidoEn.Attributes.Append(nattr)
        End If
        'If Not rdrEmisor("No_Exterior") Is System.DBNull.Value And Not rdrEmisor("No_Exterior") Is String.Empty And Not rdrEmisor("No_Exterior").ToString.Trim = "" Then
        '    nattr = Doc.CreateAttribute("noExterior")
        '    nattr.Value = rdrEmisor("No_Exterior")
        '    ExpedidoEn.Attributes.Append(nattr)
        'End If
        'If Not rdrEmisor("No_Interior") Is System.DBNull.Value And Not rdrEmisor("No_Interior") Is String.Empty And Not rdrEmisor("No_Interior").ToString.Trim = "" Then
        '    nattr = Doc.CreateAttribute("noInterior")
        '    nattr.Value = rdrEmisor("No_Interior")
        '    ExpedidoEn.Attributes.Append(nattr)
        'End If
        'If Not rdrEmisor("Colonia") Is System.DBNull.Value And Not rdrEmisor("Colonia") Is String.Empty And Not rdrEmisor("Colonia").ToString.Trim = "" Then
        '    nattr = Doc.CreateAttribute("colonia")
        '    nattr.Value = rdrEmisor("Colonia")
        '    ExpedidoEn.Attributes.Append(nattr)
        'End If
        'If Not rdrEmisor("Delegacion_Municipio") Is System.DBNull.Value And Not rdrEmisor("Delegacion_Municipio") Is String.Empty And Not rdrEmisor("Delegacion_Municipio").ToString.Trim = "" Then
        '    nattr = Doc.CreateAttribute("municipio")
        '    nattr.Value = rdrEmisor("Delegacion_Municipio")
        '    ExpedidoEn.Attributes.Append(nattr)
        'End If
        If Not rdrFactGral("STATE") Is System.DBNull.Value And Not rdrFactGral("STATE") Is String.Empty And Not rdrFactGral("STATE").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("estado")
            nattr.Value = rdrFactGral("STATE")
            ExpedidoEn.Attributes.Append(nattr)
        End If
        If Not rdrFactGral("COUNTRYNAME") Is System.DBNull.Value And Not rdrFactGral("COUNTRYNAME") Is String.Empty And Not rdrFactGral("COUNTRYNAME") = "" Then
            nattr = Doc.CreateAttribute("pais")
            nattr.Value = rdrFactGral("COUNTRYNAME")
            ExpedidoEn.Attributes.Append(nattr)
        Else
            nattr = Doc.CreateAttribute("pais")
            nattr.Value = rdrEmisor("COUNTRYNAME")
            ExpedidoEn.Attributes.Append(nattr)
        End If
        If Not rdrFactGral("ZIP") Is System.DBNull.Value And Not rdrFactGral("ZIP") Is String.Empty And Not rdrFactGral("ZIP").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("codigoPostal")
            nattr.Value = rdrFactGral("ZIP")
            ExpedidoEn.Attributes.Append(nattr)
        End If

        ' Crea nodo RegimenFiscal y agrega sus atributos
        Dim RegimenFiscal As XmlNode = Doc.CreateElement("cfdi", "RegimenFiscal", "http://www.sat.gob.mx/cfd/3")
        nattr = Doc.CreateAttribute("Regimen")
        nattr.Value = rdrEmisor("GLFO_REGIMEN")
        RegimenFiscal.Attributes.Append(nattr)

        ' Agrega el nodo ExpedidoEn dentro del nodo Emisor
        Emisor.AppendChild(ExpedidoEn)

        ' Agrega el nodo RegimenFiscal dentro del nodo Emisor
        Emisor.AppendChild(RegimenFiscal)

        ' Agrega el nodo Emisor dentro del nodo Comprobante
        Comprobante.AppendChild(Emisor)

        ' Crea nodo receptor y agrega sus atributos
        Dim Receptor As XmlNode = Doc.CreateElement("cfdi", "Receptor", "http://www.sat.gob.mx/cfd/3")
        'Dim fila1 As System.Data.DataRowView = bsReceptores.Current
        If Not rdrReceptor("VATNUM") Is System.DBNull.Value And Not rdrReceptor("VATNUM") Is String.Empty And Not rdrReceptor("VATNUM").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("rfc")
            nattr.Value = Replace(Replace(rdrReceptor("VATNUM").ToString, "-", ""), " ", "").Normalize
            Receptor.Attributes.Append(nattr)
        Else
            nattr = Doc.CreateAttribute("rfc")
            nattr.Value = IIf(rdrFactGral("CODE").ToString = "MXP", "XAXX010101000", "XEXX010101000")
            Receptor.Attributes.Append(nattr)
        End If
        nattr = Doc.CreateAttribute("nombre")
        nattr.Value = rdrReceptor("CUSTDES")
        Receptor.Attributes.Append(nattr)

        ' Crea nodo domicilio receptor y agrega sus atributos
        Dim Domicilio As XmlNode = Doc.CreateElement("cfdi", "Domicilio", "http://www.sat.gob.mx/cfd/3")
        If Not rdrReceptor("ADDRESS") Is System.DBNull.Value And Not rdrReceptor("ADDRESS") Is String.Empty And Not rdrReceptor("ADDRESS").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("calle")
            nattr.Value = rdrReceptor("ADDRESS")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("GLFO_NUMERO") Is System.DBNull.Value And Not rdrReceptor("GLFO_NUMERO") Is String.Empty And Not rdrReceptor("GLFO_NUMERO").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("noExterior")
            nattr.Value = rdrReceptor("GLFO_NUMERO")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("GLFO_MUNICIPIO") Is System.DBNull.Value And Not rdrReceptor("GLFO_MUNICIPIO") Is String.Empty And Not rdrReceptor("GLFO_MUNICIPIO").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("noInterior")
            nattr.Value = rdrReceptor("GLFO_MUNICIPIO")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("GLFO_COLONIA") Is System.DBNull.Value And Not rdrReceptor("GLFO_COLONIA") Is String.Empty And Not rdrReceptor("GLFO_COLONIA").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("colonia")
            nattr.Value = rdrReceptor("GLFO_COLONIA")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("GLFO_DELEG") Is System.DBNull.Value And Not rdrReceptor("GLFO_DELEG") Is String.Empty And Not rdrReceptor("GLFO_DELEG").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("municipio")
            nattr.Value = rdrReceptor("GLFO_DELEG")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("STATENAME") Is System.DBNull.Value And Not rdrReceptor("STATENAME") Is String.Empty And Not rdrReceptor("STATENAME").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("estado")
            nattr.Value = rdrReceptor("STATENAME")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("COUNTRYNAME") Is System.DBNull.Value And Not rdrReceptor("COUNTRYNAME") Is String.Empty And Not rdrReceptor("COUNTRYNAME").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("pais")
            nattr.Value = rdrReceptor("COUNTRYNAME")
            Domicilio.Attributes.Append(nattr)
        Else
            nattr = Doc.CreateAttribute("pais")
            nattr.Value = IIf(rdrFactGral("CODE").ToString = "MXP", "MEXICO", "E.U.A.")
            Domicilio.Attributes.Append(nattr)
        End If
        If Not rdrReceptor("ZIP") Is System.DBNull.Value And Not rdrReceptor("ZIP") Is String.Empty And Not rdrReceptor("ZIP").ToString.Trim = "" Then
            nattr = Doc.CreateAttribute("codigoPostal")
            nattr.Value = rdrReceptor("ZIP")
            Domicilio.Attributes.Append(nattr)
        End If

        ' Agrega el nodo Domicilio dentro del nodo Receptor
        Receptor.AppendChild(Domicilio)

        ' Agrega el nodo Receptor dentro del nodo Comprobante
        Comprobante.AppendChild(Receptor)

        ' Crea nodo Conceptos
        Dim Conceptos As XmlNode = Doc.CreateElement("cfdi", "Conceptos", "http://www.sat.gob.mx/cfd/3")

        ' Por cada concepto en la tabla crea un nodo Concepto y sus atributos correspondientes y los subnodos InformacionAduanera
        'Dim suma As Double = 0
        For Each Item As DataRow In rdrFactItems.Rows

            ' Crea nodo Concepto y sus atriibutos
            Dim Concepto As XmlNode = Doc.CreateElement("cfdi", "Concepto", "http://www.sat.gob.mx/cfd/3")
            nattr = Doc.CreateAttribute("cantidad")
            nattr.Value = IIf(Item("QUANT") Is System.DBNull.Value, 0, Math.Round(CDbl(Item("QUANT").ToString.Trim), 4))
            Concepto.Attributes.Append(nattr)

            If Not Item("PARTNAME") Is System.DBNull.Value And Not Item("PARTNAME").ToString Is String.Empty Then
                nattr = Doc.CreateAttribute("noIdentificacion")
                nattr.Value = Item("PARTNAME")
                Concepto.Attributes.Append(nattr)
            End If

            nattr = Doc.CreateAttribute("unidad")
            nattr.Value = Item("UNITDES")
            Concepto.Attributes.Append(nattr)

            nattr = Doc.CreateAttribute("descripcion")
            nattr.Value = IIf(Item("PARTDES2").ToString.Trim = "", Item("PARTDES"), Item("PARTDES2"))
            Concepto.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("valorUnitario")
            nattr.Value = IIf(Item("PRICE") Is System.DBNull.Value, 0, Replace(Item("PRICE"), "$", ""))
            Concepto.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("importe")
            nattr.Value = IIf(Item("QPRICE") Is System.DBNull.Value, 0, Replace(Item("QPRICE"), "$", ""))
            'suma += nattr.Value
            Concepto.Attributes.Append(nattr)

            ' Crea nodo InformacionAduanera y sus atributos
            If Not (rdrFactGral("GLFO_NPEDI").ToString Is String.Empty) And IsDate(rdrFactGral("GLFO_FECPED").ToString) And Not (rdrFactGral("GLFO_ADUANA").ToString Is String.Empty) Then
                Dim InformacionAduanera As XmlNode = Doc.CreateElement("cfdi", "InformacionAduanera", "http://www.sat.gob.mx/cfd/3")
                nattr = Doc.CreateAttribute("numero")
                nattr.Value = rdrFactGral("GLFO_NPEDI")
                InformacionAduanera.Attributes.Append(nattr)
                nattr = Doc.CreateAttribute("fecha")
                nattr.Value = Format(CDate(rdrFactGral("GLFO_FECPED")), "yyyy-MM-dd")
                InformacionAduanera.Attributes.Append(nattr)
                nattr = Doc.CreateAttribute("aduana")
                nattr.Value = rdrFactGral("GLFO_ADUANA")
                InformacionAduanera.Attributes.Append(nattr)

                ' Agrega el nodo InformacionAduanera dentro del nodo Concepto
                Concepto.AppendChild(InformacionAduanera)
            End If

            ' Crea nodo CuentaPredial
            If Not rdrFactGral("GLFO_OTRASFE") Is System.DBNull.Value And Not rdrFactGral("GLFO_OTRASFE") Is String.Empty And Not rdrFactGral("GLFO_OTRASFE").ToString.Trim = "" Then
                Dim CuentaPredial As XmlNode = Doc.CreateElement("cfdi", "CuentaPredial", "http://www.sat.gob.mx/cfd/3")
                nattr = Doc.CreateAttribute("numero")
                nattr.Value = rdrFactGral("GLFO_OTRASFE")
                CuentaPredial.Attributes.Append(nattr)

                ' Agrega el nodo CuentaPredial dentro del nodo Concepto
                Concepto.AppendChild(CuentaPredial)
            End If

            'If Not (Item("PEDIMENTO") Is System.DBNull.Value) And IsDate(Item("MARCA_D_TIEMPO")) And Not (Item("ATCUST") Is System.DBNull.Value) Then
            '    Dim InformacionAduanera As XmlNode = Doc.CreateElement("cfdi", "InformacionAduanera", "http://www.sat.gob.mx/cfd/3")
            '    nattr = Doc.CreateAttribute("numero")
            '    nattr.Value = Item("PEDIMENTO")
            '    InformacionAduanera.Attributes.Append(nattr)
            '    nattr = Doc.CreateAttribute("fecha")
            '    nattr.Value = Format(CDate(Item("MARCA_D_TIEMPO")), "yyyy-MM-dd")
            '    InformacionAduanera.Attributes.Append(nattr)
            '    nattr = Doc.CreateAttribute("aduana")
            '    nattr.Value = Item("ATCUST")
            '    InformacionAduanera.Attributes.Append(nattr)

            '    ' Agrega el nodo InformacionAduanera dentro del nodo Concepto
            '    Concepto.AppendChild(InformacionAduanera)
            'End If

            ' Agrega el nodo Concepto dentro del nodo Conceptos
            Conceptos.AppendChild(Concepto)
        Next


        ' Agrega el nodo Conceptos dentro del nodo Comprobante
        Comprobante.AppendChild(Conceptos)

        ' Crea nodo Impuestos y sus atributos
        Dim Impuestos As XmlNode = Doc.CreateElement("cfdi", "Impuestos", "http://www.sat.gob.mx/cfd/3")
        'If chkIVAtras.Checked Or chkIVAret.Checked Or chkISRret.Checked Or chkFleteRet.Checked Then
        Dim totalRetenciones As Double = 0
        Dim nattr1 As XmlAttribute



        If Not rdrFactGral("WTAX") Is System.DBNull.Value And Not rdrFactGral("WTAX") Is String.Empty And Not rdrFactGral("WTAX").ToString.Trim = "" And Not CDbl(rdrFactGral("WTAX").ToString.Trim) = 0 Then
            ' Crea nodo Retenciones por si se tiene que utilizar
            Dim Retenciones As XmlNode = Doc.CreateElement("cfdi", "Retenciones", "http://www.sat.gob.mx/cfd/3")

            ' Crea nodo Retencion y sus atributos
            Dim Retencion As XmlNode = Doc.CreateElement("cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3")
            nattr1 = Doc.CreateAttribute("impuesto")
            nattr1.Value = "IVA"
            Retencion.Attributes.Append(nattr1)
            nattr1 = Doc.CreateAttribute("importe")
            nattr1.Value = Replace(Replace(rdrFactGral("WTAX"), "$", ""), ",", "")
            Retencion.Attributes.Append(nattr1)

            ' Agrega el nodo Retencion dentro del nodo Retenciones
            Retenciones.AppendChild(Retencion)

            nattr = Doc.CreateAttribute("totalImpuestosRetenidos")
            nattr.Value = Replace(Replace(rdrFactGral("WTAX"), "$", ""), ",", "")
            Impuestos.Attributes.Append(nattr)

            ' Agrega el nodo Retenciones dentro del nodo Impuestos
            Impuestos.AppendChild(Retenciones)
        End If

        'If chkISRret.Checked Then
        '    totalRetenciones += IIf(chkISRret.Checked, Replace(txtISRret.Text, "$", ""), 0)

        '    ' Crea nodo Retencion y sus atributos
        '    Dim Retencion As XmlNode = Doc.CreateElement("cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3")
        '    nattr1 = Doc.CreateAttribute("impuesto")
        '    nattr1.Value = "ISR"
        '    Retencion.Attributes.Append(nattr1)
        '    nattr1 = Doc.CreateAttribute("importe")
        '    nattr1.Value = IIf(chkISRret.Checked, Replace(Replace(txtISRret.Text, "$", ""), ",", ""), 0)
        '    Retencion.Attributes.Append(nattr1)

        '    ' Agrega el nodo Retencion dentro del nodo Retenciones
        '    Retenciones.AppendChild(Retencion)
        'End If
        'If chkFleteRet.Checked Then
        '    totalRetenciones += IIf(chkFleteRet.Checked, Replace(txtFleteRet.Text, "$", ""), 0)

        '    ' Crea nodo Retencion y sus atributos
        '    Dim Retencion As XmlNode = Doc.CreateElement("cfdi", "Retencion", "http://www.sat.gob.mx/cfd/3")
        '    nattr1 = Doc.CreateAttribute("impuesto")
        '    nattr1.Value = "IVA"
        '    Retencion.Attributes.Append(nattr1)
        '    nattr1 = Doc.CreateAttribute("importe")
        '    nattr1.Value = IIf(chkFleteRet.Checked, Replace(Replace(txtFleteRet.Text, "$", ""), ",", ""), 0)
        '    Retencion.Attributes.Append(nattr1)

        '    ' Agrega el nodo Retencion dentro del nodo Retenciones
        '    Retenciones.AppendChild(Retencion)
        'End If

        'If totalRetenciones > 0 Then
        '    nattr = Doc.CreateAttribute("totalImpuestosRetenidos")
        '    nattr.Value = totalRetenciones
        '    Impuestos.Attributes.Append(nattr)

        '    ' Agrega el nodo Retenciones dentro del nodo Impuestos
        '    Impuestos.AppendChild(Retenciones)
        'End If

        'If chkIVAtras.Checked Then
        nattr = Doc.CreateAttribute("totalImpuestosTrasladados")
        nattr.Value = Replace(Replace(rdrFactGral("VAT"), "$", ""), ",", "")
        Impuestos.Attributes.Append(nattr)

        ' Crea nodo Traslados
        Dim Traslados As XmlNode = Doc.CreateElement("cfdi", "Traslados", "http://www.sat.gob.mx/cfd/3")
        ' Crea nodo Traslado y sus atributos
        Dim Traslado As XmlNode = Doc.CreateElement("cfdi", "Traslado", "http://www.sat.gob.mx/cfd/3")
        nattr1 = Doc.CreateAttribute("impuesto")
        nattr1.Value = "IVA"
        Traslado.Attributes.Append(nattr1)
        nattr1 = Doc.CreateAttribute("tasa")
        nattr1.Value = rdrFactGral("TAXPERCENT")
        Traslado.Attributes.Append(nattr1)
        nattr1 = Doc.CreateAttribute("importe")
        nattr1.Value = Replace(Replace(rdrFactGral("VAT"), "$", ""), ",", "")
        Traslado.Attributes.Append(nattr1)

        ' Agrega el nodo Traslado dentro del nodo Traslados
        Traslados.AppendChild(Traslado)

        ' Agrega el nodo Traslados dentro del nodo Impuestos
        Impuestos.AppendChild(Traslados)
        'End If

        ' Agrega el nodo Impuestos dentro del nodo Comprobante
        Comprobante.AppendChild(Impuestos)
        'End If

        Dim complemento As XmlNode = Doc.CreateElement("cfdi", "Complemento", "http://www.sat.gob.mx/cfd/3")
        Comprobante.AppendChild(complemento)

        Doc.AppendChild(Comprobante)
        'Doc.Save("cfdi.xml")
    End Sub

    Private Sub LlenaAddenda()
        Dim nattr As XmlAttribute

        ' Obtiene el nodo Comprobante
        Dim Comprobante As XmlNode = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0)

        ' Crea nodo Addenda
        Dim Addenda As XmlNode = Doc.CreateElement("cfdi", "Addenda", "http://www.sat.gob.mx/cfd/3")

        ' Crea nodo requestForPayment y agrega sus atributos
        Dim requestForPayment As XmlNode = Doc.CreateElement("requestForPayment")
        nattr = Doc.CreateAttribute("documentStatus")
        nattr.Value = "ORIGINAL"
        requestForPayment.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("documentStructureVersion")
        nattr.Value = "AMC7.1"
        requestForPayment.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("contentVersion")
        nattr.Value = "1.3.1"
        requestForPayment.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("type")
        nattr.Value = "SimpleInvoiceType"
        requestForPayment.Attributes.Append(nattr)


        ' Crea nodo requestForPaymentIdentification y agrega sus atributos
        Dim requestForPaymentIdentification As XmlNode = Doc.CreateElement("requestForPaymentIdentification")
        ' Crea nodo entityType y agrega sus atributos
        Dim entityType As XmlNode = Doc.CreateElement("entityType")
        entityType.InnerText = IIf(rdrFactGral("DEBIT").ToString.Trim = "D", "INVOICE", "CREDIT_NOTE")
        ' Crea nodo uniqueCreatorIdentification y agrega sus atributos
        Dim uniqueCreatorIdentification As XmlNode = Doc.CreateElement("uniqueCreatorIdentification")
        uniqueCreatorIdentification.InnerText = rdrFactGral("IVNUM").ToString

        ' Agrega el nodo entityType dentro del nodo requestForPaymentIdentification
        requestForPaymentIdentification.AppendChild(entityType)
        ' Agrega el nodo uniqueCreatorIdentification dentro del nodo requestForPaymentIdentification
        requestForPaymentIdentification.AppendChild(uniqueCreatorIdentification)
        ' Agrega el nodo uniqueCreatorIdentification dentro del nodo requestForPayment
        requestForPayment.AppendChild(requestForPaymentIdentification)



        ' Crea nodo specialIntruction y agrega sus atributos
        Dim specialIntruction As XmlNode = Doc.CreateElement("specialInstruction")
        nattr = Doc.CreateAttribute("code")
        nattr.Value = "PUR"
        specialIntruction.Attributes.Append(nattr)
        ' Crea nodo text y agrega sus atributos
        Dim text As XmlNode = Doc.CreateElement("text")
        text.InnerText = rdrReceptor("GLFO_TIPOP").ToString
        ' Agrega el nodo text dentro del nodo specialIntruction
        specialIntruction.AppendChild(text)
        ' Agrega el nodo specialIntruction dentro del nodo requestForPayment
        requestForPayment.AppendChild(specialIntruction)

        ' Crea nodo specialIntruction y agrega sus atributos
        specialIntruction = Doc.CreateElement("specialInstruction")
        nattr = Doc.CreateAttribute("code")
        nattr.Value = "ZZZ"
        specialIntruction.Attributes.Append(nattr)
        ' Crea nodo text y agrega sus atributos
        text = Doc.CreateElement("text")
        text.InnerText = ImporteConLetra(rdrFactGral("AFTERWTAX").ToString, rdrFactGral("NAME").ToString, rdrFactGral("CODE").ToString)
        ' Agrega el nodo text dentro del nodo specialIntruction
        specialIntruction.AppendChild(text)
        ' Agrega el nodo specialIntruction dentro del nodo requestForPayment
        requestForPayment.AppendChild(specialIntruction)

        ' Crea nodo specialIntruction y agrega sus atributos
        specialIntruction = Doc.CreateElement("specialInstruction")
        nattr = Doc.CreateAttribute("code")
        nattr.Value = "AAB"
        specialIntruction.Attributes.Append(nattr)
        ' Crea nodo text y agrega sus atributos
        text = Doc.CreateElement("text")
        text.InnerText = "PAGO EN UNA SOLA EXHIBICION"
        ' Agrega el nodo text dentro del nodo specialIntruction
        specialIntruction.AppendChild(text)
        ' Agrega el nodo specialIntruction dentro del nodo requestForPayment
        requestForPayment.AppendChild(specialIntruction)


        ' Crea nodo orderIdentification y agrega sus atributos
        Dim orderIdentification As XmlNode = Doc.CreateElement("orderIdentification")
        ' Crea nodo referenceIdentification y agrega sus atributos
        Dim referenceIdentification As XmlNode = Doc.CreateElement("referenceIdentification")
        referenceIdentification.InnerText = rdrFactGral("GLBF_ORDENCOMPRA").ToString
        nattr = Doc.CreateAttribute("type")
        nattr.Value = "ON"
        referenceIdentification.Attributes.Append(nattr)
        ' Crea nodo ReferenceDate y agrega sus atributos
        Dim ReferenceDate As XmlNode = Doc.CreateElement("ReferenceDate")
        ReferenceDate.InnerText = Format(rdrFactGral("GLBF_FECH_OC"), "yyyy-MM-dd")
        ' Agrega el nodo referenceIdentification dentro del nodo orderIdentification
        orderIdentification.AppendChild(referenceIdentification)
        ' Agrega el nodo ReferenceDate dentro del nodo orderIdentification
        orderIdentification.AppendChild(ReferenceDate)
        ' Agrega el nodo orderIdentification dentro del nodo requestForPayment
        requestForPayment.AppendChild(orderIdentification)


        ' Crea nodo AdditionalInformation y agrega sus atributos
        Dim AdditionalInformation As XmlNode = Doc.CreateElement("AdditionalInformation")
        ' Crea nodo referenceIdentification y agrega sus atributos
        referenceIdentification = Doc.CreateElement("referenceIdentification")
        referenceIdentification.InnerText = rdrFactGral("IVNUM").ToString
        nattr = Doc.CreateAttribute("type")
        nattr.Value = "IV"
        referenceIdentification.Attributes.Append(nattr)
        ' Agrega el nodo referenceIdentification dentro del nodo AdditionalInformation
        AdditionalInformation.AppendChild(referenceIdentification)
        ' Agrega el nodo AdditionalInformation dentro del nodo requestForPayment
        requestForPayment.AppendChild(AdditionalInformation)


        ' Crea nodo DeliveryNote y agrega sus atributos
        Dim DeliveryNote As XmlNode = Doc.CreateElement("DeliveryNote")
        ' Crea nodo referenceIdentification y agrega sus atributos
        referenceIdentification = Doc.CreateElement("referenceIdentification")
        referenceIdentification.InnerText = rdrFactGral("GLFO_FOLGR").ToString
        ' Crea nodo referenceDate y agrega sus atributos
        ReferenceDate = Doc.CreateElement("ReferenceDate")
        ReferenceDate.InnerText = Format(rdrFactGral("GLFO_FECGR"), "yyyy-MM-dd")
        ' Agrega el nodo referenceIdentification dentro del nodo DeliveryNote
        DeliveryNote.AppendChild(referenceIdentification)
        ' Agrega el nodo referenceDate dentro del nodo DeliveryNote
        DeliveryNote.AppendChild(ReferenceDate)
        ' Agrega el nodo DeliveryNote dentro del nodo requestForPayment
        requestForPayment.AppendChild(DeliveryNote)


        ' Crea nodo buyer y agrega sus atributos
        Dim buyer As XmlNode = Doc.CreateElement("buyer")
        ' Crea nodo gln y agrega sus atributos
        Dim gln As XmlNode = Doc.CreateElement("gln")
        gln.InnerText = rdrReceptor("GLFO_GLN1").ToString
        ' Agrega el nodo gln dentro del nodo buyer
        buyer.AppendChild(gln)
        ' Agrega el nodo buyer dentro del nodo requestForPayment
        requestForPayment.AppendChild(buyer)


        ' Crea nodo seller y agrega sus atributos
        Dim seller As XmlNode = Doc.CreateElement("seller")
        ' Crea nodo gln y agrega sus atributos
        gln = Doc.CreateElement("gln")
        gln.InnerText = rdrReceptor("GLFO_GLN2").ToString
        ' Crea nodo alternatePartyIdentification y agrega sus atributos
        Dim alternatePartyIdentification As XmlNode = Doc.CreateElement("alternatePartyIdentification")
        alternatePartyIdentification.InnerText = rdrReceptor("GLFO_NUMPRO").ToString
        nattr = Doc.CreateAttribute("type")
        nattr.Value = "SELLER_ASSIGNED_IDENTIFIER_FOR_A_PARTY"
        alternatePartyIdentification.Attributes.Append(nattr)
        ' Agrega el nodo gln dentro del nodo seller
        seller.AppendChild(gln)
        ' Agrega el nodo alternatePartyIdentification dentro del nodo seller
        seller.AppendChild(alternatePartyIdentification)
        ' Agrega el nodo seller dentro del nodo requestForPayment
        requestForPayment.AppendChild(seller)


        ' Crea nodo InvoiceCreator y agrega sus atributos
        Dim InvoiceCreator As XmlNode = Doc.CreateElement("InvoiceCreator")
        ' Crea nodo gln y agrega sus atributos
        gln = Doc.CreateElement("gln")
        gln.InnerText = rdrReceptor("GLFO_GLN2").ToString
        ' Crea nodo nameAndAddress y agrega sus atributos
        Dim nameAndAddress As XmlNode = Doc.CreateElement("nameAndAddress")
        ' Crea nodo name y agrega sus atributos
        Dim name As XmlNode = Doc.CreateElement("name")
        If rdrEmisor("COMPDES").ToString.Length > 35 Then
            name.InnerText = rdrEmisor("COMPDES").ToString.Substring(0, 35)
        Else
            name.InnerText = rdrEmisor("COMPDES").ToString
        End If
        ' Crea nodo streetAddressOne y agrega sus atributos
        Dim streetAddressOne As XmlNode = Doc.CreateElement("streetAddressOne")
        If (rdrEmisor("ADDRESS").ToString & " " & rdrEmisor("GLFO_NUMERO").ToString & " " & rdrEmisor("GLFO_MUNICIPIO").ToString & " " & rdrEmisor("GLFO_COLONIA").ToString).Length > 35 Then
            streetAddressOne.InnerText = (rdrEmisor("ADDRESS").ToString & " " & rdrEmisor("GLFO_NUMERO").ToString & " " & rdrEmisor("GLFO_MUNICIPIO").ToString & " " & rdrEmisor("GLFO_COLONIA").ToString).Substring(0, 35)
        Else
            streetAddressOne.InnerText = (rdrEmisor("ADDRESS").ToString & " " & rdrEmisor("GLFO_NUMERO").ToString & " " & rdrEmisor("GLFO_MUNICIPIO").ToString & " " & rdrEmisor("GLFO_COLONIA").ToString)
        End If

        ' Crea nodo city y agrega sus atributos
        Dim city As XmlNode = Doc.CreateElement("city")
        If rdrEmisor("COUNTRYNAME").ToString.Length > 35 Then
            city.InnerText = rdrEmisor("COUNTRYNAME").ToString.Substring(0, 35)
        Else
            city.InnerText = rdrEmisor("COUNTRYNAME").ToString
        End If
        ' Crea nodo postalCode y agrega sus atributos
        Dim postalCode As XmlNode = Doc.CreateElement("postalCode")
        postalCode.InnerText = rdrEmisor("ZIP").ToString
        ' Agrega el nodo gln dentro del nodo InvoiceCreator
        InvoiceCreator.AppendChild(gln)
        ' Agrega el nodo name dentro del nodo nameAndAddress
        nameAndAddress.AppendChild(name)
        ' Agrega el nodo streetAddressOne dentro del nodo nameAndAddress
        nameAndAddress.AppendChild(streetAddressOne)
        ' Agrega el nodo city dentro del nodo nameAndAddress
        nameAndAddress.AppendChild(city)
        ' Agrega el nodo postalCode dentro del nodo nameAndAddress
        nameAndAddress.AppendChild(postalCode)
        ' Agrega el nodo nameAndAddress dentro del nodo InvoiceCreator
        InvoiceCreator.AppendChild(nameAndAddress)
        ' Agrega el nodo seller dentro del nodo requestForPayment
        requestForPayment.AppendChild(InvoiceCreator)


        ' Crea nodo currency y agrega sus atributos
        Dim currency As XmlNode = Doc.CreateElement("currency")
        nattr = Doc.CreateAttribute("currencyISOCode")
        nattr.Value = "MXN" 'rdrFactGral("CODE").ToString
        currency.Attributes.Append(nattr)
        ' Crea nodo currencyFunction y agrega sus atributos
        Dim currencyFunction As XmlNode = Doc.CreateElement("currencyFunction")
        currencyFunction.InnerText = "BILLING_CURRENCY"
        ' Crea nodo rateOfChange y agrega sus atributos
        Dim rateOfChange As XmlNode = Doc.CreateElement("rateOfChange")
        rateOfChange.InnerText = rdrFactGral("EXCHANGE").ToString
        ' Agrega el nodo currencyFunction dentro del nodo currency
        currency.AppendChild(currencyFunction)
        ' Agrega el nodo rateOfChange dentro del nodo currency
        currency.AppendChild(rateOfChange)
        ' Agrega el nodo currency dentro del nodo requestForPayment
        requestForPayment.AppendChild(currency)


        ' Crea nodo paymentTerms y agrega sus atributos
        Dim paymentTerms As XmlNode = Doc.CreateElement("paymentTerms")
        nattr = Doc.CreateAttribute("PaymentTermsRelationTime")
        nattr.Value = "REFERENCE_AFTER"
        paymentTerms.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("paymentTermsEvent")
        nattr.Value = "DATE_OF_INVOICE"
        paymentTerms.Attributes.Append(nattr)
        ' Crea nodo netPayment y agrega sus atributos
        Dim netPayment As XmlNode = Doc.CreateElement("netPayment")
        nattr = Doc.CreateAttribute("netPaymentTermsType")
        nattr.Value = "BASIC_NET"
        netPayment.Attributes.Append(nattr)
        ' Crea nodo paymentTimePeriod y agrega sus atributos
        Dim paymentTimePeriod As XmlNode = Doc.CreateElement("paymentTimePeriod")
        ' Crea nodo timePeriodDue y agrega sus atributos
        Dim timePeriodDue As XmlNode = Doc.CreateElement("timePeriodDue")
        nattr = Doc.CreateAttribute("timePeriod")
        nattr.Value = "DAYS"
        timePeriodDue.Attributes.Append(nattr)
        ' Crea nodo value y agrega sus atributos
        Dim value As XmlNode = Doc.CreateElement("value")
        value.InnerText = rdrFactGral("PAYCODE").ToString
        ' Agrega el nodo value dentro del nodo timePeriodDue
        timePeriodDue.AppendChild(value)
        ' Agrega el nodo timePeriodDue dentro del nodo paymentTimePeriod
        paymentTimePeriod.AppendChild(timePeriodDue)
        ' Agrega el nodo paymentTimePeriod dentro del nodo netPayment
        netPayment.AppendChild(paymentTimePeriod)
        ' Agrega el nodo netPayment dentro del nodo paymentTerms
        paymentTerms.AppendChild(netPayment)
        ' Agrega el nodo paymentTerms dentro del nodo requestForPayment
        requestForPayment.AppendChild(paymentTerms)


        ' Crea nodo allowanceCharge y agrega sus atributos
        Dim allowanceCharge As XmlNode = Doc.CreateElement("allowanceCharge")
        nattr = Doc.CreateAttribute("settlementType")
        nattr.Value = "OFF_INVOICE"
        allowanceCharge.Attributes.Append(nattr)
        nattr = Doc.CreateAttribute("allowanceChargeType")
        nattr.Value = "ALLOWANCE_GLOBAL"
        allowanceCharge.Attributes.Append(nattr)
        ' Crea nodo specialServicesType y agrega sus atributos
        Dim specialServicesType As XmlNode = Doc.CreateElement("specialServicesType")
        specialServicesType.InnerText = "AJ"
        ' Crea nodo monetaryAmountOrPercentage y agrega sus atributos
        Dim monetaryAmountOrPercentage As XmlNode = Doc.CreateElement("monetaryAmountOrPercentage")
        ' Crea nodo rate y agrega sus atributos
        Dim rate As XmlNode = Doc.CreateElement("rate")
        nattr = Doc.CreateAttribute("base")
        nattr.Value = "INVOICE_VALUE"
        rate.Attributes.Append(nattr)
        ' Crea nodo percentage y agrega sus atributos
        Dim percentage As XmlNode = Doc.CreateElement("percentage")
        percentage.InnerText = rdrFactGral("DISPRICE").ToString
        ' Agrega el nodo percentage dentro del nodo rate
        rate.AppendChild(percentage)
        ' Agrega el nodo rate dentro del nodo monetaryAmountOrPercentage
        monetaryAmountOrPercentage.AppendChild(rate)
        ' Agrega el nodo specialServicesType dentro del nodo allowanceCharge
        allowanceCharge.AppendChild(specialServicesType)
        ' Agrega el nodo monetaryAmountOrPercentage dentro del nodo allowanceCharge
        allowanceCharge.AppendChild(monetaryAmountOrPercentage)
        ' Agrega el nodo allowanceCharge dentro del nodo requestForPayment
        requestForPayment.AppendChild(allowanceCharge)


        ' Por cada Item en la tabla crea un nodo lineItem y sus atributos
        Dim partida As Integer = 0
        For Each Item As DataRow In rdrFactItems.Rows
            partida += 1

            ' Crea nodo lineItem y agrega sus atributos
            Dim lineItem As XmlNode = Doc.CreateElement("lineItem")
            nattr = Doc.CreateAttribute("type")
            nattr.Value = "SimpleInvoiceLineItemType"
            lineItem.Attributes.Append(nattr)
            nattr = Doc.CreateAttribute("number")
            nattr.Value = partida
            lineItem.Attributes.Append(nattr)

            ' Crea nodo tradeItemIdentification y agrega sus atributos
            Dim tradeItemIdentification As XmlNode = Doc.CreateElement("tradeItemIdentification")
            ' Crea nodo gtin y agrega sus atributos
            Dim gtin As XmlNode = Doc.CreateElement("gtin")
            gtin.InnerText = "***"
            ' Agrega el nodo gtin dentro del nodo tradeItemIdentification
            tradeItemIdentification.AppendChild(gtin)
            ' Agrega el nodo tradeItemIdentification dentro del nodo lineItem
            lineItem.AppendChild(tradeItemIdentification)

            ' Crea nodo tradeItemDescriptionInformation y agrega sus atributos
            Dim tradeItemDescriptionInformation As XmlNode = Doc.CreateElement("tradeItemDescriptionInformation")
            nattr = Doc.CreateAttribute("language")
            nattr.Value = "ES"
            tradeItemDescriptionInformation.Attributes.Append(nattr)
            ' Crea nodo longText y agrega sus atributos
            Dim longText As XmlNode = Doc.CreateElement("longText")
            If Item("PARTDES").ToString.Length > 35 Then
                longText.InnerText = Item("PARTDES").ToString.Substring(0, 35)
            Else
                longText.InnerText = Item("PARTDES").ToString
            End If
            ' Agrega el nodo longText dentro del nodo tradeItemDescriptionInformation
            tradeItemDescriptionInformation.AppendChild(longText)
            ' Agrega el nodo tradeItemIdentification dentro del nodo lineItem
            lineItem.AppendChild(tradeItemDescriptionInformation)

            ' Crea nodo invoicedQuantity y agrega sus atributos
            Dim invoicedQuantity As XmlNode = Doc.CreateElement("invoicedQuantity")
            invoicedQuantity.InnerText = Item("QUANT").ToString
            nattr = Doc.CreateAttribute("unitOfMeasure")
            nattr.Value = Item("UNITNAME").ToString
            invoicedQuantity.Attributes.Append(nattr)
            ' Agrega el nodo invoicedQuantity dentro del nodo lineItem
            lineItem.AppendChild(invoicedQuantity)

            ' Crea nodo grossPrice y agrega sus atributos
            Dim grossPrice As XmlNode = Doc.CreateElement("grossPrice")
            ' Crea nodo Amount y agrega sus atributos
            Dim Amount1 As XmlNode = Doc.CreateElement("Amount")
            Amount1.InnerText = Item("PRICE").ToString
            ' Agrega el nodo Amount dentro del nodo grossPrice
            grossPrice.AppendChild(Amount1)
            ' Agrega el nodo grossPrice dentro del nodo lineItem
            lineItem.AppendChild(grossPrice)

            ' Crea nodo netPrice y agrega sus atributos
            Dim netPrice As XmlNode = Doc.CreateElement("netPrice")
            ' Crea nodo Amount y agrega sus atributos
            Amount1 = Doc.CreateElement("Amount")
            Amount1.InnerText = Item("PRICE").ToString
            ' Agrega el nodo Amount dentro del nodo netPrice
            netPrice.AppendChild(Amount1)
            ' Agrega el nodo netPrice dentro del nodo lineItem
            lineItem.AppendChild(netPrice)

            ' Crea nodo tradeItemTaxInformation y agrega sus atributos
            Dim tradeItemTaxInformation As XmlNode = Doc.CreateElement("tradeItemTaxInformation")
            ' Crea nodo taxTypeDescription y agrega sus atributos
            Dim taxTypeDescription As XmlNode = Doc.CreateElement("taxTypeDescription")
            taxTypeDescription.InnerText = "VAT"
            ' Crea nodo tradeItemTexAmount y agrega sus atributos
            Dim tradeItemTexAmount As XmlNode = Doc.CreateElement("tradeItemTaxAmount")
            ' Crea nodo taxPercentage y agrega sus atributos
            Dim taxPercentage1 As XmlNode = Doc.CreateElement("taxPercentage")
            taxPercentage1.InnerText = rdrFactGral("TAXPERCENT").ToString
            ' Crea nodo taxAmount y agrega sus atributos
            Dim taxAmount1 As XmlNode = Doc.CreateElement("taxAmount")
            taxAmount1.InnerText = rdrFactGral("VAT").ToString
            ' Agrega el nodo taxPercentage dentro del nodo tradeItemTexAmount
            tradeItemTexAmount.AppendChild(taxPercentage1)
            ' Agrega el nodo taxAmount dentro del nodo tradeItemTexAmount
            tradeItemTexAmount.AppendChild(taxAmount1)
            ' Agrega el nodo tradeItemTexAmount dentro del nodo tradeItemTaxInformation
            tradeItemTaxInformation.AppendChild(tradeItemTexAmount)
            ' Agrega el nodo taxTypeDescription dentro del nodo tradeItemTaxInformation
            tradeItemTaxInformation.AppendChild(taxTypeDescription)
            ' Agrega el nodo tradeItemTaxInformation dentro del nodo lineItem
            lineItem.AppendChild(tradeItemTaxInformation)

            ' Crea nodo totalLineAmount y agrega sus atributos
            Dim totalLineAmount As XmlNode = Doc.CreateElement("totalLineAmount")
            ' Crea nodo grossAmount y agrega sus atributos
            Dim grossAmount As XmlNode = Doc.CreateElement("grossAmount")
            ' Crea nodo Amount y agrega sus atributos
            Amount1 = Doc.CreateElement("Amount")
            Amount1.InnerText = Item("QPRICE").ToString
            ' Agrega el nodo Amount dentro del nodo grossAmount
            grossAmount.AppendChild(Amount1)
            ' Crea nodo netAmount y agrega sus atributos
            Dim netAmount As XmlNode = Doc.CreateElement("netAmount")
            ' Crea nodo Amount y agrega sus atributos
            Amount1 = Doc.CreateElement("Amount")
            Amount1.InnerText = Item("QPRICE").ToString
            ' Agrega el nodo Amount dentro del nodo netAmount
            netAmount.AppendChild(Amount1)
            ' Agrega el nodo grossAmount dentro del nodo totalLineAmount
            totalLineAmount.AppendChild(grossAmount)
            ' Agrega el nodo netAmount dentro del nodo totalLineAmount
            totalLineAmount.AppendChild(netAmount)
            ' Agrega el nodo totalLineAmount dentro del nodo lineItem
            lineItem.AppendChild(totalLineAmount)

            ' Agrega el nodo allowanceCharge dentro del nodo requestForPayment
            requestForPayment.AppendChild(lineItem)
        Next


        ' Crea nodo totalAmount y agrega sus atributos
        Dim totalAmount As XmlNode = Doc.CreateElement("totalAmount")
        ' Crea nodo Amount y agrega sus atributos
        Dim Amount As XmlNode = Doc.CreateElement("Amount")
        Amount.InnerText = rdrFactGral("DISPRICE").ToString
        ' Agrega el nodo Amount dentro del nodo totalAmount
        totalAmount.AppendChild(Amount)
        ' Agrega el nodo totalAmount dentro del nodo requestForPayment
        requestForPayment.AppendChild(totalAmount)


        ' Crea nodo TotalAllowanceCharge y agrega sus atributos
        Dim TotalAllowanceCharge As XmlNode = Doc.CreateElement("TotalAllowanceCharge")
        nattr = Doc.CreateAttribute("allowanceOrChargeType")
        nattr.Value = "ALLOWANCE"
        TotalAllowanceCharge.Attributes.Append(nattr)
        ' Crea nodo Amount y agrega sus atributos
        Amount = Doc.CreateElement("Amount")
        Amount.InnerText = rdrFactGral("DISCOUNT").ToString
        ' Crea nodo specialServicesType y agrega sus atributos
        specialServicesType = Doc.CreateElement("specialServicesType")
        specialServicesType.InnerText = "AJ"
        ' Agrega el nodo specialServicesType dentro del nodo TotalAllowanceCharge
        TotalAllowanceCharge.AppendChild(specialServicesType)
        ' Agrega el nodo Amount dentro del nodo TotalAllowanceCharge
        TotalAllowanceCharge.AppendChild(Amount)
        ' Agrega el nodo totalAmount dentro del nodo requestForPayment
        requestForPayment.AppendChild(TotalAllowanceCharge)


        ' Crea nodo baseAmount y agrega sus atributos
        Dim baseAmount As XmlNode = Doc.CreateElement("baseAmount")
        ' Crea nodo Amount y agrega sus atributos
        Amount = Doc.CreateElement("Amount")
        Amount.InnerText = rdrFactGral("DISPRICE").ToString
        ' Agrega el nodo Amount dentro del nodo baseAmount
        baseAmount.AppendChild(Amount)
        ' Agrega el nodo baseAmount dentro del nodo requestForPayment
        requestForPayment.AppendChild(baseAmount)


        ' Crea nodo tax y agrega sus atributos
        Dim tax As XmlNode = Doc.CreateElement("tax")
        ' Crea nodo taxPercentage y agrega sus atributos
        Dim taxPercentage As XmlNode = Doc.CreateElement("taxPercentage")
        taxPercentage.InnerText = rdrFactGral("TAXPERCENT").ToString
        ' Crea nodo taxAmount y agrega sus atributos
        Dim taxAmount As XmlNode = Doc.CreateElement("taxAmount")
        taxAmount.InnerText = rdrFactGral("VAT").ToString
        ' Agrega el nodo taxPercentage dentro del nodo tax
        tax.AppendChild(taxPercentage)
        ' Agrega el nodo taxAmount dentro del nodo tax
        tax.AppendChild(taxAmount)
        ' Agrega el nodo tax dentro del nodo requestForPayment
        requestForPayment.AppendChild(tax)


        ' Crea nodo payableAmount y agrega sus atributos
        Dim payableAmount As XmlNode = Doc.CreateElement("payableAmount")
        ' Crea nodo Amount y agrega sus atributos
        Amount = Doc.CreateElement("Amount")
        Amount.InnerText = rdrFactGral("AFTERWTAX").ToString
        ' Agrega el nodo Amount dentro del nodo baseAmount
        payableAmount.AppendChild(Amount)
        ' Agrega el nodo payableAmount dentro del nodo requestForPayment
        requestForPayment.AppendChild(payableAmount)


        ' Agrega el nodo requestForPayment dentro del nodo Addenda
        Addenda.AppendChild(requestForPayment)

        ' Agrega el nodo Addenda dentro del nodo Comprobante
        Comprobante.AppendChild(Addenda)
    End Sub

    Private Function GeneraQueryComprobante()
        Dim strQuery As String = "INSERT INTO comprobantes.dbo.Comprobante (Fecha, XML, IV, Company, Estatus) VALUES('Fecha_', @XML, IV_, 'Company_', 'Estatus_')"

        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        Doc.Save(writer)
        msXML.Position = 0

        strQuery = Replace(strQuery, "Fecha_", Format(Now, "yyyy-MM-dd HH:mm:ss"))
        'strQuery = Replace(strQuery, "XML_", System.Text.Encoding.Unicode.GetString(msXML.ToArray))
        strQuery = Replace(strQuery, "IV_", rdrFactGral("IV").ToString)
        strQuery = Replace(strQuery, "Company_", Empresa)
        strQuery = Replace(strQuery, "Estatus_", "T")

        Return strQuery
    End Function

    Private Sub EnviaCorreo(ByVal Ruta As String)
        Dim html As String = ""
        Console.WriteLine("Generando correo")
        Try
            html = System.IO.File.ReadAllText("Correo.htm")
            Dim nombreEmpresa As String = rdrEmisor("COMPDES")
            Dim dirEmpresa As String = String.Format("RFC: {0} | {1} {2} {3} {4} {5} {6} {7} {8}", rdrEmisor("VATNUM"), rdrEmisor("ADDRESS"), rdrEmisor("GLFO_NUMERO"), rdrEmisor("GLFO_MUNICIPIO"), rdrEmisor("GLFO_COLONIA"), rdrEmisor("ZIP"), rdrEmisor("GLFO_DELEG"), rdrEmisor("STATENAME"), rdrEmisor("COUNTRYNAME"))
            Dim fecha As String = String.Format("{0:F}", Now.Date)
            Dim nombreCliente As String = rdrReceptor("CUSTDES")
            Dim dirCliente As String = String.Format("RFC: {0} | {1} {2} {3} {4} {5} {6} {7} {8}", rdrReceptor("VATNUM"), rdrReceptor("ADDRESS"), rdrReceptor("GLFO_NUMERO"), rdrReceptor("GLFO_MUNICIPIO"), rdrReceptor("GLFO_COLONIA"), rdrReceptor("ZIP"), rdrReceptor("GLFO_DELEG"), rdrReceptor("STATENAME"), rdrReceptor("COUNTRYNAME"))
            html = Replace(html, "NOMBRE_EMPRESA", nombreEmpresa)
            html = Replace(html, "DIRECCION_EMPRESA", dirEmpresa)
            html = Replace(html, "FECHA", fecha)
            html = Replace(html, "NOMBRE_CLIENTE", nombreCliente)
            html = Replace(html, "DIRECCION_CLIENTE", dirCliente)
            html = Replace(html, "MENSAJE_PERSONALIZADO", "")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Exit Sub
        End Try

        Dim rdrConfig As SqlDataReader = Datos.RegresaReader("SELECT * FROM comprobantes.dbo.SysConfig")
        Try
            Dim attPDF As New Attachment(Replace(Ruta, ".xml", ".pdf"))
            Dim attXML As New Attachment(Ruta)
            Dim Correo As New MailMessage
            Dim Cliente As New SmtpClient
            rdrConfig.Read()

            Correo.To.Add(rdrReceptor("EMAIL").ToString.Trim)
            If Not rdrReceptor("GLOB_EMAIL") Is System.DBNull.Value And Not rdrReceptor("GLOB_EMAIL") Is String.Empty And Not rdrReceptor("GLOB_EMAIL").ToString.Trim = "" Then
                Correo.CC.Add(rdrReceptor("GLOB_EMAIL").ToString.Trim)
            End If
            Correo.From = New MailAddress(rdrConfig("Mail").ToString.Trim)
            Correo.Subject = "CFDI " & rdrReceptor("VATNUM") & " " & rdrFactGral("IVNUM")
            Correo.IsBodyHtml = True
            Correo.Body = html
            Correo.Attachments.Add(attPDF)
            Correo.Attachments.Add(attXML)

            Cliente.Host = rdrConfig("host").ToString.Trim
            Cliente.Port = rdrConfig("Puerto")
            Cliente.Credentials = New System.Net.NetworkCredential(rdrConfig("Mail").ToString.Trim, rdrConfig("Password").ToString.Trim)
            Console.WriteLine("Enviando correo")
            Cliente.Send(Correo)
            Correo.Dispose()
            Cliente.Dispose()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Exit Sub
        Finally
            rdrConfig.Close()
        End Try
        Console.WriteLine("El correo ha sido enviado")
    End Sub

    Private Sub GeneraReporte(ByRef dsResultados As DataSet, ByVal Ruta As String)
        ' declarar objetos
        Dim oRptPrueba As ReportDocument
        Dim oConexInfo As ConnectionInfo
        Dim oListaTablas As Tables
        Dim oTabla As Table
        Dim oTablaConexInfo As TableLogOnInfo

        ' instanciar objeto para guardar datos de conexión
        oConexInfo = New ConnectionInfo()
        oConexInfo.ServerName = "ds_cfdv3"
        oConexInfo.DatabaseName = ""
        oConexInfo.UserID = ""
        oConexInfo.Password = ""
        Console.WriteLine("Paso")
        ' instanciar objeto informe
        oRptPrueba = New ReportDocument
        If reciboPago Then
            oRptPrueba.FileName = "Recibo.rpt"
        ElseIf Tras = False Then
            oRptPrueba.FileName = "predeterminadoSinIVA.rpt"
        ElseIf System.IO.File.Exists(rdrCSD("RutaCFDIper").ToString) Then
            oRptPrueba.FileName = rdrCSD("RutaCFDIper").ToString
        ElseIf Retencion Then
            oRptPrueba.FileName = "ConRetencionIVA.rpt"
        Else
            oRptPrueba.FileName = "predeterminado.rpt"
        End If

        oRptPrueba.SetDataSource(dsResultados)

        ' obtener la colección de tablas del informe
        oListaTablas = oRptPrueba.Database.Tables

        ' por cada tabla del informe...
        For Each oTabla In oListaTablas
            ' ...obtener el objeto con los datos de conexión
            oTablaConexInfo = oTabla.LogOnInfo
            ' asignar el objeto con datos de conexión
            ' que hemos creado
            oTablaConexInfo.ConnectionInfo = oConexInfo
            ' aplicar cambios de conexión a la tabla
            oTabla.ApplyLogOnInfo(oTablaConexInfo)
        Next
        oRptPrueba.ExportToDisk(ExportFormatType.PortableDocFormat, Replace(Ruta, ".xml", ".pdf"))

        Dim archivoPFD As New System.IO.StreamReader(Replace(Ruta, ".xml", ".pdf"), True)

        Dim con As New SqlConnection(Datos.ConnectionString)
        con.Open()
        Dim trans As SqlTransaction = con.BeginTransaction()
        Dim cmd As New SqlCommand()
        cmd.Connection = con
        cmd.Transaction = trans

        Try
            cmd.Parameters.Add("@PDF", SqlDbType.Image)
            cmd.Parameters.Item("@PDF").Value = System.IO.File.ReadAllBytes(Replace(Ruta, ".xml", ".pdf"))
            cmd.CommandText = "UPDATE comprobantes.dbo.Comprobante SET PDF=@PDF WHERE IV=" & rdrFactGral("IV").ToString & " AND Company='" & Empresa & "'"
            cmd.ExecuteNonQuery()
            trans.Commit()
        Catch ex As Exception
            trans.Rollback()
            Console.WriteLine("Error al generar el registro del comprobante!")
            Exit Sub
        End Try
    End Sub
    Sub AgregaImpuesto(ByRef Doc As XmlDocument, nodo As XmlNode, base As Double, impuesto As String, TipoFactor As String, TasaOCuota As String, importe As Double)
        AppendAttributeXML(nodo, Doc.CreateAttribute("Base"), base)
        AppendAttributeXML(nodo, Doc.CreateAttribute("Impuesto"), impuesto)
        AppendAttributeXML(nodo, Doc.CreateAttribute("TipoFactor"), TipoFactor)
        AppendAttributeXML(nodo, Doc.CreateAttribute("TasaOCuota"), FormatNumber(TasaOCuota, 6))
        AppendAttributeXML(nodo, Doc.CreateAttribute("Importe"), FormatNumber(Convert.ToDouble(importe), 6, , , Microsoft.VisualBasic.TriState.False))
    End Sub

    Public Sub AppendAttributeXML(ByRef Nodo As XmlNode, ByVal Atributo As XmlAttribute, ByVal Valor As String)
        If Valor <> "" Then
            Atributo.Value = Valor
            Nodo.Attributes.Append(Atributo)
        End If
    End Sub

    Public Sub modificaXMLRecibo(ByRef Doc As XmlDocument)
        Dim nattr As XmlAttribute
        Doc.GetElementsByTagName("Concepto", "http://www.sat.gob.mx/cfd/3")(0).ParentNode.RemoveChild(Doc.GetElementsByTagName("Concepto", "http://www.sat.gob.mx/cfd/3")(0))
        For Each filaPago As DataRow In rdrFactPagadas.Rows
            If filaPago("CREDIT1") > 0 Then
                Dim Concepto As XmlNode = Doc.CreateElement("cfdi", "Concepto", "http://www.sat.gob.mx/cfd/3")
                nattr = Doc.CreateAttribute("Cantidad")
                nattr.Value = "1"
                Concepto.Attributes.Append(nattr)

                nattr = Doc.CreateAttribute("NoIdentificacion")
                nattr.Value = "01"
                Concepto.Attributes.Append(nattr)

                nattr = Doc.CreateAttribute("ClaveUnidad")
                nattr.Value = "ACT"
                Concepto.Attributes.Append(nattr)

                nattr = Doc.CreateAttribute("Descripcion")
                nattr.Value = "Prueba Recibo"
                Concepto.Attributes.Append(nattr)
                nattr = Doc.CreateAttribute("ValorUnitario")
                nattr.Value = "100"
                Concepto.Attributes.Append(nattr)
                nattr = Doc.CreateAttribute("Importe")
                nattr.Value = "100"
                Concepto.Attributes.Append(nattr)
                nattr = Doc.CreateAttribute("ClaveProdServ")
                nattr.Value = "01010101"
                Concepto.Attributes.Append(nattr)
                Doc.GetElementsByTagName("Conceptos", "http://www.sat.gob.mx/cfd/3")(0).AppendChild(Concepto)
            End If
        Next
    End Sub
End Module
