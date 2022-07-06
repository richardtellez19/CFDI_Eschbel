Imports System.Text
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports System.Net
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.Xsl
Imports System.Xml.XPath
Imports System.Xml.Schema
Imports SW.Services.Stamp
Imports SW.Services.Cancelation
Imports SW.Services.Status

Public Class BD
    Dim wsLicencia As New LicenciasSAF.Service1
    Dim UTF8withoutBOM As Encoding = New UTF8Encoding(False)
    Dim validSchema As Boolean = True
    Dim _CantUsuarios As Integer = 0
    Dim _ConnString As String = ""
    Dim _Usr As String = ""
    Dim _RutaDocs As String = ""
    'llave pública y privada. para encriptar y desencriptar
    Private cadena As String = "<RSAKeyValue><Modulus>mfvgKAhO5wlvRID5rH2rTrQ/9UxmOgBZ1r85FEOrDzjhLoa+FpUpu8wYch9kZWWqcpipVGE6Fqh0B2A/399owWgKqN" & _
                               "uX233YZfHYiwuumRN+EyiA+Yu9+/t1x1f+Po7SqnaLfJ79sLdtY8BB8ZZv7EtIoyneeWSZkiwfAiaEpdM=</Modulus><Exponent>AQAB</Exponent>" & _
                               "<P>2bQuFm8yr6IYcM4c1dR0gWCHH+Ch0h3blWkh75bMH9vNQSBxBBUo6Kr0DXkpgM9j4csF/fOYvKar0bwrS7OVEw==</P><Q>tRI0zP9epYZBRMiqW87" & _
                               "xIIODm0lDro3thi0DAJKwjJ3p85flHAzLia7RkqcKuqcDU50x7yjiqUAnFO3rznqEQQ==</Q><DP>wDjRgMmKTX2oauHyn/NJM3sRlFn5R9dJLTF4312c" & _
                               "unyPTPPiFiLAzj8z4jmbJbr4jEOA9OwDbn8Tssqcx+i2tQ==</DP><DQ>CWsuBe/Oq4uFP5+0hNwJ3OgaY2x3MdA+J7mVq2T1/AqGzd370+6yc7H9QsSf" & _
                               "oT/ow2Os4kTdLXKh1cvedToYQQ==</DQ><InverseQ>BtRoYo14UEhYRjGcVxLZ+oOCplyNEHDuQK4iPYKt4aTNIgjSmuX1BXF9++QrXY0tJppxoYKx87" & _
                               "u9ObQuxeh8dA==</InverseQ><D>AsNaSl2bzNmGfZwy3r2CjW7+ltBAJVpMQmY8B/kO4L8Mi2EvmAglL5GB/spBavBgFp5Wq4TzHeT38AnyG3pOzuJ/L" & _
                               "yI822/PeOi75V/MQ6gOM4iCv4FTWkZaJPfoAXKmVfcgathr6Pz0VSoOcO9o+tJhrIrykDQ50OTWLEGUbAE=</D></RSAKeyValue>"

    Sub New()

    End Sub

    Sub New(ByVal ConnStr As String)
        _ConnString = ConnStr
    End Sub

    Sub New(ByVal ConnStr As String, ByVal RutaDocs As String)
        _ConnString = ConnStr
        _RutaDocs = RutaDocs
    End Sub

    Public Property Usuarios() As Integer
        Get
            Return _CantUsuarios
        End Get
        Set(ByVal value As Integer)
            _CantUsuarios = IIf(IsNumeric(value), value, 0)
        End Set
    End Property

    Public Property UsrActual() As String
        Get
            Return _Usr
        End Get
        Set(ByVal value As String)
            _Usr = value
        End Set
    End Property

    Public Property ConnectionString() As String
        Get
            Return _ConnString
        End Get
        Set(ByVal value As String)
            _ConnString = value
        End Set
    End Property

    Public Property RutaDocs() As String
        Get
            Return _RutaDocs
        End Get
        Set(ByVal value As String)
            _RutaDocs = value
        End Set
    End Property

    Public Function RegresaReader(ByVal query As String) As SqlDataReader
        Dim conn As New SqlConnection(_ConnString)

        Try
            conn.Open()
            Dim cmd As New SqlCommand(query, conn)
            cmd.CommandType = CommandType.Text
            Return cmd.ExecuteReader(CommandBehavior.CloseConnection)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return Nothing
        End Try


    End Function

    Public Function GetDataScalar(ByRef Query As String) As Object
        Dim con As New SqlConnection(_ConnString)

        Try
            con.Open()
        Catch ex As Exception
            Return Nothing
        End Try

        Dim cmd As New SqlCommand(Query, con)
        cmd.CommandType = CommandType.Text

        Try
            Return cmd.ExecuteScalar
        Catch ex As Exception
            con.Close()
            Return Nothing
        End Try
    End Function

    Public Function UpdateDB(ByVal Query As String) As Boolean
        Dim con As New SqlConnection(Datos.ConnectionString)
        con.Open()
        Dim trans As SqlTransaction = con.BeginTransaction()
        Dim cmd As New SqlCommand()
        cmd.Connection = con
        cmd.Transaction = trans

        Try
            cmd.CommandText = Query
            cmd.ExecuteNonQuery()
            trans.Commit()
        Catch ex As Exception
            trans.Rollback()
            Console.WriteLine(ex.Message, MsgBoxStyle.Exclamation & vbCrLf)
            Return False
        Finally
            con.Close()
        End Try

        Return True
    End Function

    'encripta la cadena recibida
    Public Function Encripta(ByVal texto As String) As String
        Dim textbytes, encryptedtextbytes As Byte()
        Dim rsa As New RSACryptoServiceProvider()
        Dim encoder As New UTF8Encoding

        rsa.FromXmlString(cadena)
        Dim resultado As String = String.Empty
        textbytes = encoder.GetBytes(texto)
        'encrypt the text
        encryptedtextbytes = rsa.Encrypt(textbytes, True)
        Return Convert.ToBase64String(encryptedtextbytes)
    End Function

    'desencripta la cadena recibida
    Public Function Desencripta(ByVal texto As String) As String
        Dim textbytes As Byte()
        Dim rsa As New RSACryptoServiceProvider()
        Dim encoder As New UTF8Encoding

        rsa.FromXmlString(cadena) 'se crea el objeto de encripción RSA a partir del xml que contiene las llaves pública y privada
        Dim resultado As String = String.Empty
        Try
            'recuperar bytes de la cadena de texto (base 64)
            textbytes = Convert.FromBase64String(texto)
            'get the decrypted clear text byte array
            textbytes = rsa.Decrypt(textbytes, True)
            'convert the byte array to text using the same encoding format that was used for encryption
            resultado = encoder.GetString(textbytes)
        Catch ex As Exception
            resultado = String.Empty
        End Try
        Return resultado
    End Function

    Public Function GetTipoCFDI(ByVal Tipo As String) As String
        '    FACTURA()
        '    CARTA PORTE()
        '    NOTA DE CARGO()
        '    NOTA DE CREDITO()
        '    RECIBO DE DONATIVO()
        '    RECIBO DE PAGO()
        '    BOLETA DE EMPEÑO()
        '    NOTA DE DEVOLUCION()
        '    BONIFICACION NOTA DE CONSUMO()
        '    COMPROBANTE DE PAGO A PLAZOS()
        '    ESTADO DE CUENTA COMBUSTIBLES()

        If Tipo = "FACTURA" Or Tipo = "NOTA DE CARGO" Or Tipo = "RECIBO DE DONATIVO" Or Tipo = "RECIBO DE PAGO" Or Tipo = "BOLETA DE EMPEÑO" Or Tipo = "COMPROBANTE DE PAGO A PLAZOS" Then
            Return "ingreso"
        End If

        If Tipo = "NOTA DE CREDITO" Or Tipo = "NOTA DE DEVOLUCION" Or Tipo = "BONIFICACION NOTA DE CONSUMO" Then
            Return "egreso"
        End If

        If Tipo = "CARTA PORTE" Then
            Return "traslado"
        End If

        Return "ingreso"
    End Function

    Function compruebaTimbresRestantes(ByVal Emisor As String) As Boolean
        'Dim wsLicencia As New LicenciasSAF.Service1
        Dim respuesta As Boolean = False
        Dim intentos As Integer = 0

        While intentos < 4
            Try
                respuesta = wsLicencia.compruebaTimbresRestantes(Emisor)
                Exit While
            Catch ex As Exception
                intentos += 1
                respuesta = False
                Exit Try
            End Try
        End While

        Return respuesta
    End Function

    Public Function ActualizaSysInfo(ByVal Emisor As String) As Boolean
        'Dim wsLicencia As New LicenciasSAF.Service1
        Dim respuesta As Boolean = False
        Dim intentos As Integer = 0

        While intentos < 4
            Try
                respuesta = wsLicencia.ActualizaSysInfo(Emisor)
                If Not respuesta Then
                    Console.WriteLine("¡Hubo un error en el registro del comprobante!" & vbCr & "Favor de ponerse en contacto con Mobile-Metriks", MsgBoxStyle.Exclamation)
                End If
                Exit While
            Catch ex As Exception
                intentos += 1
                respuesta = False
                Exit Try
            End Try
        End While

        Return respuesta


    End Function

    Public Function ObtieneSysInfo(ByVal Emisor As String) As String
        'Dim wsLicencia As New LicenciasSAF.Service1
        Dim respuesta As String = ""
        Dim intentos As Integer = 0

        While intentos < 4
            Try
                respuesta = wsLicencia.ObtieneSysInfo(Emisor)
                Exit While
            Catch ex As Exception
                intentos += 1
                respuesta = ""
                Exit Try
            End Try
        End While

        Return respuesta
    End Function

    Public Function ObtieneStatusFolios(ByVal Emisor As String) As DataSet
        'Dim wsLicencia As New LicenciasSAF.Service1
        Dim ds As DataSet

        Try
            ds = wsLicencia.StatusFolios(Emisor)
            Return ds
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function ObtienePreciosFolios() As DataSet
        'Dim wsLicencia As New LicenciasSAF.Service1
        Dim ds As DataSet

        Try
            ds = wsLicencia.TablaPrecios()
            Return ds
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function SolicitaFolios(RFC As String, Licencia As String, CantidadFolios As Integer) As Boolean
        'Dim wsLicencia As New LicenciasSAF.Service1

        Try
            Return wsLicencia.SolicitarFolios(CantidadFolios, Licencia, RFC, False)
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function ReemplazaCaracteres(ByVal cadena As String) As String
        cadena = Replace(cadena, "&amp;", "&")
        cadena = Replace(cadena, "&quot;", """")
        cadena = Replace(cadena, "&lt;", "<")
        cadena = Replace(cadena, "&gt;", ">")
        cadena = Replace(cadena, "&apos;", "'")

        Return cadena
    End Function

    'Public Sub Firma() 'ByRef Doc As XmlDocument, ByRef rdrCSD As SqlDataReader)
    '    ' Convierte a memorystream
    '    Dim msXML As MemoryStream = New MemoryStream
    '    Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
    '    Doc.Save(writer)
    '    'Console.WriteLine(Encoding.UTF8.GetString(msXML.ToArray()))

    '    ' Carga el certificado
    '    Dim cert As X509Certificate2 = New X509Certificate2(rdrCSD("CSD").ToString.Trim, rdrCSD("Password").ToString.Trim)

    '    ' Numero de serie
    '    Dim serialNumber As String = ""
    '    Dim carr As String = cert.GetSerialNumberString
    '    Dim i As Int16
    '    For i = 1 To carr.Length
    '        If i Mod 2 = 1 Then
    '            serialNumber += carr(i)
    '        End If
    '    Next

    '    Console.WriteLine("El certificado vence el: " & cert.GetExpirationDateString)

    '    ' Certificado tipo PEM
    '    Dim certPEM As String = System.Convert.ToBase64String(cert.Export(X509ContentType.Cert))

    '    ' Genera la cadena original
    '    Dim msChain As MemoryStream = New MemoryStream()
    '    Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
    '    Dim xslt As XslCompiledTransform = New XslCompiledTransform()
    '    xslt.Load("cadenaoriginal_3_2.xslt")
    '    msXML.Position = 0
    '    Dim xp As XPathDocument = New XPathDocument(msXML)
    '    xslt.Transform(xp, tw)
    '    'xslt.Transform("cfdi.xml", "cadorig.txt")

    '    Dim CadOrig As String = ReemplazaCaracteres(UTF8withoutBOM.GetString(msChain.ToArray()))
    '    ' Firma con la llave privada
    '    Dim sha1 As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider()
    '    msChain.Position = 0
    '    Dim rsa1 As RSACryptoServiceProvider = cert.PrivateKey
    '    'Dim sello As String = Convert.ToBase64String(rsa1.SignData(msChain.ToArray(), sha1))
    '    Dim sello As String = Convert.ToBase64String(rsa1.SignData(UTF8withoutBOM.GetBytes(CadOrig.ToCharArray), sha1))

    '    ' Integra los elementos en el nodo Comprobante
    '    Dim nattr As XmlAttribute = Doc.CreateAttribute("noCertificado")
    '    nattr.Value = serialNumber
    '    Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes.Append(nattr)
    '    nattr = Doc.CreateAttribute("certificado")
    '    nattr.Value = certPEM
    '    Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes.Append(nattr)
    '    nattr = Doc.CreateAttribute("sello")
    '    nattr.Value = sello
    '    Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes.Append(nattr)
    'End Sub

    'Public Function Valida() 'ByRef doc As XmlDocument) As Boolean
    '    ' Inicializa variable de retorno
    '    Dim ret As Boolean = True

    '    ' Valida contra esquema
    '    Dim eventHandler As ValidationEventHandler = New ValidationEventHandler(AddressOf ValidationEventHandler)
    '    'Doc.Schemas.Add("http://www.sat.gob.mx/cfd/3", "cfdv32.xsd")
    '    Doc.Schemas.Add("http://www.sat.gob.mx/cfd/3", "INVOIC-AMECE-XML-XSD-V7.1.xsd")
    '    Doc.Validate(eventHandler)
    '    ' Afecta la variable global validSchema
    '    If validSchema Then
    '        Console.WriteLine("OK - Esquema válido.")
    '    Else
    '        Return False
    '    End If

    '    ' Convierte a memorystream
    '    Dim cert As X509Certificate2
    '    Dim msXML As MemoryStream = New MemoryStream
    '    Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
    '    Doc.Save(writer)
    '    ' Carga el certificado
    '    Dim att As XmlAttributeCollection = Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes
    '    Try
    '        cert = New X509Certificate2(Convert.FromBase64String(att("certificado").Value))
    '    Catch ex As Exception
    '        Console.WriteLine(ex)
    '        Return False
    '    End Try
    '    If cert.HasPrivateKey Then
    '        Console.WriteLine("Error - El certificado contiene una llave privada.")
    '        ret = False
    '    Else
    '        Console.WriteLine("OK - El certificado no contiene una llave privada.")
    '    End If
    '    ' Numero de serie
    '    Dim serialNumber As String = ""
    '    Dim carr As String = cert.GetSerialNumberString
    '    Dim i As Int16
    '    For i = 1 To carr.Length
    '        If i Mod 2 = 1 Then
    '            serialNumber += carr(i)
    '        End If
    '    Next
    '    If serialNumber = att("noCertificado").Value Then
    '        Console.WriteLine("OK - El número de certificado es correcto.")
    '    Else
    '        Console.WriteLine("Error - No coincide el número de certificado.")
    '        ret = False
    '    End If
    '    ' Genera la cadena original
    '    Dim msChain As MemoryStream = New MemoryStream()
    '    Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
    '    Dim xslt As XslCompiledTransform = New XslCompiledTransform()
    '    xslt.Load("cadenaoriginal_3_2.xslt")
    '    msXML.Position = 0
    '    Dim xp As XPathDocument = New XPathDocument(msXML)
    '    xslt.Transform(xp, tw)
    '    Dim CadOrig As String = ReemplazaCaracteres(UTF8withoutBOM.GetString(msChain.ToArray()))
    '    ' Verifica con la llave publica contenida en el certificado
    '    Dim sha1 As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider()
    '    msChain.Position = 0
    '    Dim rsa1 As RSACryptoServiceProvider = cert.PublicKey.Key
    '    'If rsa1.VerifyData(msChain.ToArray(), sha1, Convert.FromBase64String(att("sello").Value)) Then
    '    If rsa1.VerifyData(UTF8withoutBOM.GetBytes(CadOrig.ToCharArray), sha1, Convert.FromBase64String(att("sello").Value)) Then
    '        Console.WriteLine("OK - Sello Digital válido.")
    '    Else
    '        Console.WriteLine("Error - El sello digital es inválido.")
    '        ret = False
    '    End If
    '    Return ret
    'End Function

    Public Sub Firma3_3(ByVal Params As FirmaParams)
        ' Convierte a memorystream
        ' Dim msXML As MemoryStream = New MemoryStream
        'Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        ' Doc.Save(writer)
        'Console.WriteLine(Encoding.UTF8.GetString(msXML.ToArray()))
        'Console.WriteLine(rdrCSD("CSD").ToString.Trim)
        ' Carga el certificado
        Dim cert As X509Certificate2 = New X509Certificate2(rdrCSD("CSD").ToString.Trim, rdrCSD("Password").ToString.Trim, X509KeyStorageFlags.Exportable)

        ' Numero de serie
        Dim serialNumber As String = ""
        Dim carr As String = cert.GetSerialNumberString
        Dim i As Int16
        For i = 1 To carr.Length
            If i Mod 2 = 1 Then
                serialNumber += carr(i)
            End If
        Next

        Console.WriteLine("El certificado vence el: " & cert.GetExpirationDateString)

        ' Certificado tipo PEM
        Dim certPEM As String = System.Convert.ToBase64String(cert.Export(X509ContentType.Cert))
        Dim nattr As XmlAttribute = Doc.CreateAttribute(Params.AtrNoCert)
        nattr.Value = serialNumber
        Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes.Append(nattr)
        nattr = Doc.CreateAttribute(Params.AtrCertif)
        nattr.Value = certPEM
        Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes.Append(nattr)

        ' Convierte a memorystream
        Dim msXML As New MemoryStream
        Dim writer As New XmlTextWriter(msXML, UTF8withoutBOM)
        Doc.Save(writer)
        ' Genera la cadena original
        Dim msChain As MemoryStream = New MemoryStream()
        Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
        Dim xslt As XslCompiledTransform = New XslCompiledTransform()
        xslt.Load(My.Application.Info.DirectoryPath & "\" & Params.ArchivoXSLT)
        msXML.Position = 0
        Dim xp As XPathDocument = New XPathDocument(msXML)
        xslt.Transform(xp, tw)
        Dim CadOrig As String = ReemplazaCaracteres(UTF8withoutBOM.GetString(msChain.ToArray()))
        ' Firma con la llave privada
        Dim privateKey As RSACryptoServiceProvider = cert.PrivateKey
        Dim privateKey1 As RSACryptoServiceProvider = New RSACryptoServiceProvider()
        privateKey1.ImportParameters(privateKey.ExportParameters(True))

        'Dim sha1 As SHA256CryptoServiceProvider = New SHA256CryptoServiceProvider()
        msChain.Position = 0
        'Dim rsa1 As RSACryptoServiceProvider = Certificado.PrivateKey
        Dim sello As String = Convert.ToBase64String(privateKey1.SignData(UTF8withoutBOM.GetBytes(CadOrig.ToCharArray), "SHA256"))
        'Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).Attributes("Sello").Value = sello

        nattr = Doc.CreateAttribute(Params.AtrSello)
        nattr.Value = sello
        Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes.Append(nattr)
    End Sub

    Public Sub Firma(Params As FirmaParams)
        ' Convierte a memorystream
        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        Doc.Save(writer)
        'Console.WriteLine(Encoding.UTF8.GetString(msXML.ToArray()))

        ' Carga el certificado
        Dim cert As X509Certificate2 = New X509Certificate2(rdrCSD("CSD").ToString.Trim, rdrCSD("Password").ToString.Trim)

        ' Numero de serie
        Dim serialNumber As String = ""
        Dim carr As String = cert.GetSerialNumberString
        Dim i As Int16
        For i = 1 To carr.Length
            If i Mod 2 = 1 Then
                serialNumber += carr(i)
            End If
        Next

        Console.WriteLine("El certificado vence el: " & cert.GetExpirationDateString)

        ' Certificado tipo PEM
        Dim certPEM As String = System.Convert.ToBase64String(cert.Export(X509ContentType.Cert))

        ' Genera la cadena original
        Dim msChain As MemoryStream = New MemoryStream()
        Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
        Dim xslt As XslCompiledTransform = New XslCompiledTransform()
        'xslt.Load(My.Application.Info.DirectoryPath & "\cadenaoriginal_3_2.xslt")
        xslt.Load(My.Application.Info.DirectoryPath & "\" & Params.ArchivoXSLT)
        msXML.Position = 0
        Dim xp As XPathDocument = New XPathDocument(msXML)
        xslt.Transform(xp, tw)
        'xslt.Transform("cfdi.xml", "cadorig.txt")

        Dim CadOrig As String = ReemplazaCaracteres(UTF8withoutBOM.GetString(msChain.ToArray()))
        ' Firma con la llave privada
        Dim sha1 As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider()
        msChain.Position = 0
        Dim rsa1 As RSACryptoServiceProvider = cert.PrivateKey
        'Dim sello As String = Convert.ToBase64String(rsa1.SignData(msChain.ToArray(), sha1))
        Dim sello As String = Convert.ToBase64String(rsa1.SignData(UTF8withoutBOM.GetBytes(CadOrig.ToCharArray), sha1))

        ' Integra los elementos en el nodo Comprobante
        Dim nattr As XmlAttribute = Doc.CreateAttribute(Params.AtrNoCert)
        nattr.Value = serialNumber
        Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes.Append(nattr)
        nattr = Doc.CreateAttribute(Params.AtrCertif)
        nattr.Value = certPEM
        Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes.Append(nattr)
        nattr = Doc.CreateAttribute(Params.AtrSello)
        nattr.Value = sello
        Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes.Append(nattr)
    End Sub

    Public Function Valida(Params As FirmaParams) ' ArchivoXSD As String, ArchivoXSLT As String, Nodo As String, EspacioNombres As String) 'ByRef doc As XmlDocument) As Boolean
        ' Inicializa variable de retorno
        Dim ret As Boolean = True

        ' Valida contra esquema
        Dim eventHandler As ValidationEventHandler = New ValidationEventHandler(AddressOf ValidationEventHandler)
        Doc.Schemas.Add(Params.EspacioNombres, My.Application.Info.DirectoryPath & "\" & Params.ArchivoXSD)
        Doc.Validate(eventHandler)
        ' Afecta la variable global validSchema
        If validSchema Then
            Console.WriteLine("OK - Esquema válido.")
        Else
            'Return False
        End If

        ' Convierte a memorystream
        Dim cert As X509Certificate2
        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        Doc.Save(writer)

        ' Carga el certificado
        Dim att As XmlAttributeCollection = Doc.GetElementsByTagName(Params.NodoCert, Params.EspacioNombres)(0).Attributes
        Try
            cert = New X509Certificate2(Convert.FromBase64String(att(Params.AtrCertif).Value))
        Catch ex As Exception
            Console.WriteLine(ex)
            Return False
        End Try

        If cert.HasPrivateKey Then
            Console.WriteLine("Error - El certificado contiene una llave privada.")
            ret = False
        Else
            Console.WriteLine("OK - El certificado no contiene una llave privada.")
        End If
        ' Numero de serie
        Dim serialNumber As String = ""
        Dim carr As String = cert.GetSerialNumberString
        Dim i As Int16
        For i = 1 To carr.Length
            If i Mod 2 = 1 Then
                serialNumber += carr(i)
            End If
        Next
        If serialNumber = att(Params.AtrNoCert).Value Then
            Console.WriteLine("OK - El número de certificado es correcto.")
        Else
            Console.WriteLine("Error - No coincide el número de certificado.")
            ret = False
        End If
        ' Genera la cadena original
        Dim msChain As MemoryStream = New MemoryStream()
        Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
        Dim xslt As XslCompiledTransform = New XslCompiledTransform()
        xslt.Load(My.Application.Info.DirectoryPath & "\" & Params.ArchivoXSLT)
        msXML.Position = 0
        Dim xp As XPathDocument = New XPathDocument(msXML)
        xslt.Transform(xp, tw)
        Dim CadOrig As String = ReemplazaCaracteres(UTF8withoutBOM.GetString(msChain.ToArray()))
        ' Verifica con la llave publica contenida en el certificado
        Dim sha1 As SHA1CryptoServiceProvider = New SHA1CryptoServiceProvider()
        msChain.Position = 0
        Dim rsa1 As RSACryptoServiceProvider = cert.PublicKey.Key
        'If rsa1.VerifyData(msChain.ToArray(), sha1, Convert.FromBase64String(att("sello").Value)) Then
        If rsa1.VerifyData(UTF8withoutBOM.GetBytes(CadOrig.ToCharArray), sha1, Convert.FromBase64String(att(Params.AtrSello).Value)) Then
            Console.WriteLine("OK - Sello Digital válido.")
        Else
            Console.WriteLine("Error - El sello digital es inválido.")
            'ret = False
        End If
        Return ret
    End Function

    Public Function TimbraEDICOM() 'ByRef doc As XmlDocument, ByRef CadenaOriginal_TFD As String)
        'Try
        '    File.Delete("cfdi.xml")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        'Try
        '    File.Delete("cfdi.zip")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        Dim docSinAddenda As XmlDocument = Doc
        Dim addenda As XmlNode = Nothing
        Try
            Console.WriteLine("Extrayendo la Addenda...")
            addenda = docSinAddenda.GetElementsByTagName("Addenda", "http://www.sat.gob.mx/cfd/3")(0)
            docSinAddenda.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).RemoveChild(addenda)
            Console.WriteLine("OK - Se extrajo la Addenda.")
        Catch ex As Exception
            addenda = Nothing
            Console.WriteLine("OK - El documento no contiene Addenda.")
        End Try

        'doc.Save("cfdi.xml")
        Dim wsEDICOM As New com.sedeb2b.cfdiws.CFDiService
        'Dim msXMLXIP As New MemoryStream
        'Dim zip As New Ionic.Zip.ZipFile("cfdi.zip")
        'Try
        '    File.Copy("cfdi.xml", "cfdi.xml", True)
        'Catch ex As Exception
        '    Exit Try
        'End Try

        'zip.AddFile("cfdi.xml")
        'zip.Save()
        Dim arrzip As Byte()

        Try
            Dim msXML1 As MemoryStream = New MemoryStream
            Dim writer1 As XmlTextWriter = New XmlTextWriter(msXML1, UTF8withoutBOM)
            docSinAddenda.Save(writer1)
            msXML1.Position = 0

            If TestFlag Then
                arrzip = wsEDICOM.getCfdiTest("MOB100617FNA", "ewyfndkpm", msXML1.ToArray)
            Else
                arrzip = wsEDICOM.getCfdi("MOB100617FNA", "ewyfndkpm", msXML1.ToArray)
            End If
        Catch ex As Exception
            Console.WriteLine("Error - " & ex.Message)
            Return False
        End Try

        Console.WriteLine("OK - Timbrado exitoso.")
        Console.WriteLine("Actualizando registro...")

        'Try
        '    File.Delete("cfdit.zip")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        'Try
        '    Directory.Delete("Timbre", True)
        'Catch ex As Exception
        '    Exit Try
        'End Try
        'Console.WriteLine("Aqui")
        ActualizaSysInfo(rdrEmisor("VATNUM"))

        Console.WriteLine("OK - Registro actualizado con éxito.")

        'File.WriteAllBytes("cfdit.zip", arrzip)

        Dim xmlTimZIP As Ionic.Zip.ZipFile = Ionic.Zip.ZipFile.Read(arrzip) 'Ionic.Zip.ZipFile.Read("cfdit.zip")
        xmlTimZIP.ExtractAll("Timbre", Ionic.Zip.ExtractExistingFileAction.OverwriteSilently)
        xmlTimZIP.Dispose()
        Doc = New XmlDocument
        Doc.Load("Timbre\SIGN_XML_COMPROBANTE_3_0.xml")

        If Not IsNothing(addenda) Then
            Dim addendaNueva As XmlNode = Doc.ImportNode(addenda, True)
            Doc.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).AppendChild(addendaNueva)
        End If

        Console.WriteLine("Generando cadena original del TFD...")
        ' Convierte a memorystream
        Dim msXML_TFD As MemoryStream = New MemoryStream
        Dim writer_TFD As XmlTextWriter = New XmlTextWriter(msXML_TFD, UTF8withoutBOM)
        Dim timbre As XmlNode
        If Not reciboPago Then
            timbre = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).FirstChild
        Else
            timbre = Doc.GetElementsByTagName("Complemento", "http://www.sat.gob.mx/cfd/3")(0).ChildNodes(1)
        End If
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

        'Try
        '    File.Delete("cfdi.xml")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        'Try
        '    File.Delete("cfdi.zip")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        'Try
        '    File.Delete("cfdit.zip")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        Try
            Directory.Delete("Timbre", True)
        Catch ex As Exception
            Exit Try
        End Try

        Return True
    End Function

    Public Function TimbraSW() As Boolean
        Dim docSinAddenda As XmlDocument = Doc
        Dim addenda As XmlNode = Nothing

        Try
            Console.WriteLine("Extrayendo la Addenda...")
            addenda = docSinAddenda.GetElementsByTagName("Addenda", "http://www.sat.gob.mx/cfd/3")(0)
            docSinAddenda.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).RemoveChild(addenda)
            Console.WriteLine("OK - Se extrajo la Addenda.")
        Catch ex As Exception
            addenda = Nothing
            Console.WriteLine("OK - El documento no contiene Addenda.")
        End Try
        If File.Exists(Datos.RutaDocs & "\Timbre" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & "\SIGN_XML_COMPROBANTE_3_0.xml") Then
            Console.WriteLine("Se omitió el timbrado. El XML ya había sido timbrado.")
            docSinAddenda = New XmlDocument
            docSinAddenda.Load(Datos.RutaDocs & "\Timbre" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & "\SIGN_XML_COMPROBANTE_3_0.xml")
        Else
            Dim url As String = "http://services.sw.com.mx"
            Dim user As String = "richard@mobilemetriks.com"
            Dim pass As String = "moBile18."
            If TestFlag Then
                url = "http://services.test.sw.com.mx"
                'user = "javier@mobilemetriks.com"
                pass = "mobile18"
            End If
            Try
                Dim stamp As New Stamp(url, user, pass)
                Dim xml As String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(docSinAddenda.OuterXml))
                Dim response As StampResponseV3 = stamp.TimbrarV3(xml, True)
                If Not response.data Is Nothing Then
                    docSinAddenda.LoadXml(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(response.data.cfdi)))
                    Console.WriteLine("OK - CFDI Timbrado")
                Else
                    Console.WriteLine(response.message & " " & response.messageDetail)
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine(ex.ToString)
                Return False
            End Try

            Try
                Console.WriteLine("Actualizando registro...")
                ActualizaSysInfo(Doc.GetElementsByTagName("Emisor", "http://www.sat.gob.mx/cfd/3")(0).Attributes("Rfc").Value)
                Console.WriteLine("OK - Registro actualizado con éxito.")
            Catch ex As Exception
                Console.WriteLine("Error al actualizar folio")
                Console.Read()
            End Try
        End If
        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        docSinAddenda.Save(writer)
        msXML.Position = 0
        Doc = New XmlDocument
        Doc = docSinAddenda

        Console.WriteLine("Generando cadena original del TFD...")
        ' Convierte a memorystream
        Dim msXML_TFD As MemoryStream = New MemoryStream
        Dim writer_TFD As XmlTextWriter = New XmlTextWriter(msXML_TFD, UTF8withoutBOM)
        Dim timbre As XmlNode = Doc.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0)
        Dim docTimbre As New XmlDocument()
        docTimbre.CreateXmlDeclaration("1.0", "UTF-8", "")
        docTimbre.AppendChild(docTimbre.ImportNode(timbre, True))
        docTimbre.Save(writer_TFD)

        'For i = 1 To 5
        Dim msChain As MemoryStream = New MemoryStream()

        Try
            Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
            Dim xslt As XslCompiledTransform = New XslCompiledTransform()
            xslt.Load("cadenaoriginal_TFD_1_1.xslt")
            msXML_TFD.Position = 0
            Dim xp As XPathDocument = New XPathDocument(msXML_TFD)
            xslt.Transform(xp, tw)
            CadenaOriginal_TFD = System.Text.Encoding.ASCII.GetString(msChain.ToArray).Normalize
            Console.WriteLine("OK - Cadena original del TFD generada con éxito.")
            'Exit For
        Catch ex As Exception
            Console.WriteLine("Error - No se pudo generar la cadena original del TFD.")
            Exit Try
        End Try
        Return True
    End Function

    Public Function TimbraFINKOK() As Boolean
        ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol Or CType(3072, SecurityProtocolType)
        Dim docSinAddenda As XmlDocument = Doc
        Dim addenda As XmlNode = Nothing
        Dim MensajeIncidencia As String = ""
        Dim CodigoIncidencia As String = ""

        Try
            Console.WriteLine("Extrayendo la Addenda...")
            addenda = docSinAddenda.GetElementsByTagName("Addenda", "http://www.sat.gob.mx/cfd/3")(0)
            docSinAddenda.GetElementsByTagName("Comprobante", "http://www.sat.gob.mx/cfd/3")(0).RemoveChild(addenda)
            Console.WriteLine("OK - Se extrajo la Addenda.")
        Catch ex As Exception
            addenda = Nothing
            Console.WriteLine("OK - El documento no contiene Addenda.")
        End Try
        If File.Exists(Datos.RutaDocs & "\Timbre" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & "\SIGN_XML_COMPROBANTE_3_0.xml") Then
            Console.WriteLine("Se omitió el timbrado. El XML ya había sido timbrado.")
            docSinAddenda = New XmlDocument
            docSinAddenda.Load(Datos.RutaDocs & "\Timbre" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & "\SIGN_XML_COMPROBANTE_3_0.xml")
        Else
            Try
                Console.WriteLine()
                'Dim SelloSAT As String = ""
                'Dim noCertificadoSAT As String = ""
                'Dim FechaTimbrado As String = ""
                'Dim uuid As String = ""
                'Dim xmlTimbrado As String = ""
                Dim xmlCfdi As New System.Xml.XmlDocument()
                Dim timbrado As New Finkok.StampSOAP ' cfdi2.com.finkok.demofacturacion.StampSOAP
                Dim timb As New Finkok.stamp ' stamp
                Dim ResponseTimbrar As New Finkok.stampResponse ' stampResponse
                'xmlCfdi.Load(XML) 'Cargamos el archivo XML.
                timb.xml = Convert.FromBase64String(Convert.ToBase64String(UTF8withoutBOM.GetBytes(docSinAddenda.OuterXml))) ' stringToBase64ByteArray(Doc.OuterXml) 'El archivo XML se envia en Base64.
                timb.username = "direccion@mobilemetriks.com"
                timb.password = "B9h/Ww8q"
                'Las siguientes lineas de codigo son para obtener la petición soap REQUEST de timbrado
                'Dim url As String
                'Dim usuario As String
                'usuario = Environment.UserName
                'url = "C:\Users\" & usuario & "\Documents\"
                'Dim XML = New StreamWriter(Application.StartupPath & "\SOAP_EnvelopTimbrado.xml")     'Dirección donde guardaremos el SOAP Envelope
                Dim XML As New MemoryStream
                Dim soap = New Serialization.XmlSerializer(timb.GetType())    'Obtenemos los datos del SOAP de la variable timb
                soap.Serialize(XML, timb)            'Serializa el timbrado y escribe el documento XML con el archivo con Stream especificado.
                XML.Close()
                'Mandamos llamar al web services y se envían los parámetros en la variable timb
                ResponseTimbrar = timbrado.stamp(timb)
                If DirectCast(ResponseTimbrar.stampResult, Finkok.AcuseRecepcionCFDI).Incidencias.Length > 0 Then
                    CodigoIncidencia = DirectCast(ResponseTimbrar.stampResult, Finkok.AcuseRecepcionCFDI).Incidencias(0).CodigoError
                    MensajeIncidencia = DirectCast(ResponseTimbrar.stampResult, Finkok.AcuseRecepcionCFDI).Incidencias(0).MensajeIncidencia
                Else
                    CodigoIncidencia = "000"
                    MensajeIncidencia = "Timbrado Satisfactorio"
                End If

                If CodigoIncidencia = "000" Then
                    'uuid = ResponseTimbrar.stampResult.UUID
                    'xmlTimbrado = ResponseTimbrar.stampResult.xml
                    'FechaTimbrado = ResponseTimbrar.stampResult.Fecha
                    'SelloSAT = ResponseTimbrar.stampResult.SatSeal
                    'noCertificadoSAT = ResponseTimbrar.stampResult.NoCertificadoSAT
                    docSinAddenda.LoadXml(ResponseTimbrar.stampResult.xml) 'xmlCfdi.LoadXml(xmlTimbrado)
                    'xmlCfdi.Save(XML)
                    'txtUUID.Text = uuid
                ElseIf CodigoIncidencia = "707" Then
                    Dim ResponseIsTimbrado As New Finkok.stampedResponse
                    Dim isTimbrado As New Finkok.stamped
                    isTimbrado.xml = Convert.FromBase64String(Convert.ToBase64String(UTF8withoutBOM.GetBytes(xmlCfdi.OuterXml)))
                    isTimbrado.username = "direccion@mobilemetriks.com"
                    isTimbrado.password = "&2Bdk2yA"
                    ResponseIsTimbrado = timbrado.stamped(isTimbrado)
                    'uuid = ResponseIsTimbrado.stampedResult.UUID
                    'xmlTimbrado = ResponseIsTimbrado.stampedResult.xml
                    'FechaTimbrado = ResponseIsTimbrado.stampedResult.Fecha
                    'SelloSAT = ResponseIsTimbrado.stampedResult.SatSeal
                    'noCertificadoSAT = ResponseIsTimbrado.stampedResult.NoCertificadoSAT
                    docSinAddenda.LoadXml(ResponseIsTimbrado.stampedResult.xml) 'xmlCfdi.LoadXml(xmlTimbrado)
                    'xmlCfdi.Save(XML)
                    'txtUUID.Text = uuid
                Else
                    'MessageBox.Show(MensajeIncidencia)
                    'ErrorXML = MensajeIncidencia
                    'TimbreBackgroundWorker.ReportProgress(0)
                    'TimbreBackgroundWorker.CancelAsync()
                    Console.WriteLine(CodigoIncidencia)
                    Console.WriteLine(MensajeIncidencia)
                    Return False
                End If

                'MessageBox.Show("Timbrado exitoso.")

            Catch ex As Exception
                Console.WriteLine("No se realizo el timbrado " & ex.Message)
                Console.WriteLine(CodigoIncidencia)
                'ErrorXML = ex.Message
                'TimbreBackgroundWorker.ReportProgress(0)
                'TimbreBackgroundWorker.CancelAsync()
                Return False
            Finally
                'Windows.Forms.Cursor.Current = Cursors.Default
            End Try
            Console.WriteLine(CodigoIncidencia)
            Console.WriteLine(MensajeIncidencia)

            'TimbreBackgroundWorker.ReportProgress(6)
            Try
                Console.WriteLine("Actualizando registro...")
                ActualizaSysInfo(Doc.GetElementsByTagName("Emisor", "http://www.sat.gob.mx/cfd/3")(0).Attributes("Rfc").Value)
                'ActualizaSysInfo(rdrEmisor("GLB_RFCGLOBAL").ToString) '(Doc.GetElementsByTagName("Emisor", "http://www.sat.gob.mx/cfd/3")(0).Attributes("rfc").Value)
                Console.WriteLine("OK - Registro actualizado con éxito.")
            Catch ex As Exception
                Console.WriteLine("Error al actualizar folio")
                Console.Read()
            End Try
        End If
        Dim msXML As MemoryStream = New MemoryStream
        Dim writer As XmlTextWriter = New XmlTextWriter(msXML, UTF8withoutBOM)
        docSinAddenda.Save(writer)
        msXML.Position = 0
        Doc = New XmlDocument
        Doc = docSinAddenda

        'Doc.Load(Datos.RutaDocs & "\Timbre" & rdrEmisor("VATNUM").ToString.Trim & rdrFactGral("IVNUM").ToString.Trim & "\SIGN_XML_COMPROBANTE_3_0.xml")

        'TimbreBackgroundWorker.ReportProgress(7)
        Console.WriteLine("Generando cadena original del TFD...")
        ' Convierte a memorystream
        Dim msXML_TFD As MemoryStream = New MemoryStream
        Dim writer_TFD As XmlTextWriter = New XmlTextWriter(msXML_TFD, UTF8withoutBOM)
        Dim timbre As XmlNode = Doc.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0)
        Dim docTimbre As New XmlDocument()
        docTimbre.CreateXmlDeclaration("1.0", "UTF-8", "")
        docTimbre.AppendChild(docTimbre.ImportNode(timbre, True))
        docTimbre.Save(writer_TFD)

        'For i = 1 To 5
        Dim msChain As MemoryStream = New MemoryStream()

        Try
            Dim tw As XmlTextWriter = New XmlTextWriter(msChain, UTF8withoutBOM)
            Dim xslt As XslCompiledTransform = New XslCompiledTransform()
            xslt.Load("cadenaoriginal_TFD_1_1.xslt")
            msXML_TFD.Position = 0
            Dim xp As XPathDocument = New XPathDocument(msXML_TFD)
            xslt.Transform(xp, tw)
            CadenaOriginal_TFD = System.Text.Encoding.ASCII.GetString(msChain.ToArray).Normalize
            Console.WriteLine("OK - Cadena original del TFD generada con éxito.")
            'Exit For
        Catch ex As Exception
            Console.WriteLine("Error - No se pudo generar la cadena original del TFD.")
            Exit Try
        End Try
        'Next

        'If DsCatalogos.TimbreFiscal.Rows.Count = 0 Then
        '    DsCatalogos.TimbreFiscal.Rows.Add(docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("RfcProvCertif").Value,
        '                                      docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("SelloSAT").Value,
        '                                      docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("NoCertificadoSAT").Value,
        '                                      docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("FechaTimbrado").Value,
        '                                      New Guid(docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("UUID").Value),
        '                                      CadenaOriginal_TFD)
        'Else
        '    DsCatalogos.TimbreFiscal(0).RfcProvCertif = docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("RfcProvCertif").Value
        '    DsCatalogos.TimbreFiscal(0).SelloSAT = docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("SelloSAT").Value
        '    DsCatalogos.TimbreFiscal(0).NoCertificadoSAT = docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("NoCertificadoSAT").Value
        '    DsCatalogos.TimbreFiscal(0).FechaTimbrado = docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("FechaTimbrado").Value
        '    DsCatalogos.TimbreFiscal(0).UUID = New Guid(docTimbre.GetElementsByTagName("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital")(0).Attributes("UUID").Value)
        '    DsCatalogos.TimbreFiscal(0).CadenaOriginalTFD = CadenaOriginal_TFD
        'End If

        'Doc.Save("test.xml")

        'Try
        '    File.Delete(Application.StartupPath & "\SOAP_EnvelopTimbrado.xml")
        'Catch ex As Exception
        '    Exit Try
        'End Try

        Return True
    End Function

    Public Sub CancelaSW(ByVal UUID As String, totalCFDI As Double, ByVal IV As String, ByVal Emp As String, ByVal motivo As String, ByVal uuidRel As String)

        'Declaramos URL y credenciales para consumir el servicio
        Dim url As String = "http://services.sw.com.mx"
        Dim user As String = "richard@mobilemetriks.com"
        Dim pass As String = "moBile18."
        'Si está en modo de pruebas cambiamos URL y clave
        If TestFlag Then
            url = "http://services.test.sw.com.mx"
            pass = "mobile18"
        End If
        'Variables necesarias
        Dim rfcEmisor As String = rdrEmisor("VATNUM").ToString.Trim
        Dim rfcReceptor As String = rdrReceptor("VATNUM").ToString.Trim

        Dim Cer As String = Convert.ToBase64String(File.ReadAllBytes(rdrCSD("CSD").ToString.Trim.Replace("pfx", "cer")))
        Dim Key As String = Convert.ToBase64String(File.ReadAllBytes(rdrCSD("RutaKey").ToString.Trim))
        Dim passKey As String = rdrCSD("Password").ToString.Trim
        Dim status As New Status("https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc")
        'Verificamos si aún no se ha realizado solicitud de cancelación
        'If rdrFactGral("GLFO_CANCELPEND") <> "Y" Then
        'Caso en que se realizará la solicitu de cancelación
        Console.WriteLine("Iniciando solicitud de cancelación...")
        Console.WriteLine("Cancelando folio fiscal: " & UUID)
        Console.WriteLine("Por favor espere...")
        Try
            Dim cancelation = New Cancelation(url, user, pass)
            Dim response As CancelationResponse
            If motivo = "01" Then
                response = cancelation.CancelarByCSD(Cer, Key, rfcEmisor, passKey, UUID, motivo, uuidRel)
            Else
                response = cancelation.CancelarByCSD(Cer, Key, rfcEmisor, passKey, UUID, motivo)
            End If
            Dim codigo As String = ""

            Try
                response.data.uuid.TryGetValue(UUID, codigo)
            Catch ex As Exception
                Console.WriteLine(response.messageDetail)
                Console.WriteLine("Presiona enter para continuar...")
                Exit Sub
            End Try

            If rdrFactGral("GLFO_CANCELPEND") <> "Y" Then
                If response.status = "success" And Not response.data Is Nothing Then
                    'If codigo = "201" Or codigo = "202" Then
                    Dim fechaCancelacion As Date = Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now)
                    Dim fechaInicial As New DateTime(1988, 1, 1, 0, 0, 0)
                    Dim fechaCancelacionBigInt = DateDiff(DateInterval.Day, fechaInicial, fechaCancelacion) * 1440

                    Try
                        Dim responseStatus0 = status.GetStatusCFDI(rfcEmisor, rfcReceptor, totalCFDI, UUID)
                        If responseStatus0.Estado = "Cancelado" Then
                            Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLFO_CANCELPEND='Y', GLOB_CANCELADA='Y', GLFO_CANCAUTH='Y', GLFO_FECHSOLCANC = " & fechaCancelacionBigInt & "  WHERE IV=" & rdrFactGral("IV"))
                            Datos.UpdateDB("UPDATE comprobantes.dbo.Comprobante SET Estatus='C' WHERE IV=" & IV & " AND Company='" & Emp & "'")
                            Console.WriteLine("Ok - El CFDI se canceló exitosamente")
                            Console.WriteLine("Presiona enter para continuar...")
                            Exit Sub
                        Else
                            Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLFO_CANCELPEND='Y', GLFO_FECHSOLCANC = " & fechaCancelacionBigInt & "  WHERE IV=" & rdrFactGral("IV"))
                            Datos.UpdateDB("UPDATE comprobantes.dbo.Comprobante SET Estatus='P' WHERE IV=" & IV & " AND Company='" & Emp & "'")
                            Console.WriteLine("Ok - Se realizó la solicitud de cancelación")
                        End If
                    Catch ex As Exception
                        Console.WriteLine(ex.ToString)
                    End Try
                    'Console.WriteLine("Presiona enter para continuar...")
                Else
                    Console.WriteLine("No se pudo realizar la solicitud de cancelación, favor de intentar más tarde. Detalles:")
                    Console.WriteLine(response.message & " " & response.messageDetail)
                End If
            End If
        Catch ex As Exception
            Console.WriteLine("Error al Cancelar" & ex.ToString)
        End Try        'Else

        'End If
    End Sub

    Public Sub StatusSAT(ByVal UUID As String, totalCFDI As Double, ByVal IV As String, ByVal Emp As String)
        Dim rfcEmisor As String = rdrEmisor("VATNUM").ToString.Trim
        Dim rfcReceptor As String = rdrReceptor("VATNUM").ToString.Trim
        Dim status As New Status("https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc")
        Console.WriteLine("Revisando estado de cancelación...")
        Dim responseStatus = Status.GetStatusCFDI(rfcEmisor, rfcReceptor, totalCFDI, UUID)
        'Console.WriteLine(responseStatus.Estado & " " & responseStatus.EstatusCancelacion)
        'responseStatus.
        If responseStatus.Estado = "Cancelado" Or responseStatus.EstatusCancelacion = "Plazo vencido" Or responseStatus.EstatusCancelacion.Contains("Cancelado") Then
            Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLOB_CANCELADA='Y', GLFO_CANCAUTH='Y' WHERE IV=" & rdrFactGral("IV"))
            Datos.UpdateDB("UPDATE comprobantes.dbo.Comprobante SET Estatus='C' WHERE IV=" & IV & " AND Company='" & Emp & "'")
            Console.WriteLine("El CFDI se canceló exitosamente - " & responseStatus.EstatusCancelacion)
            Console.WriteLine("Presiona enter para continuar...")
        ElseIf responseStatus.EstatusCancelacion = "Solicitud rechazada" Then
            Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLFO_CANCELPEND='', GLFO_CANCRECH='Y' WHERE IV=" & rdrFactGral("IV"))
            Datos.UpdateDB("UPDATE comprobantes.dbo.Comprobante SET Estatus='T' WHERE IV=" & IV & " AND Company='" & Emp & "'")
            Console.WriteLine("El receptor rechazó la cancelación del CDFI, será necesario solicitar la cancelación nuevamente.")
            Console.WriteLine("Presiona enter para continuar...")
        Else
            Console.WriteLine("Estatus en el SAT: " + responseStatus.Estado + vbCrLf + "Estatus Cancelación: " + responseStatus.EstatusCancelacion)
            Console.WriteLine("Presiona enter para continuar...")
        End If
        Console.Read()
    End Sub

    Public Function CancelaFinkok(ByVal arrUUID As String(), totalCFDI As Double) As Boolean
        ServicePointManager.SecurityProtocol = CType(3072, SecurityProtocolType) Or SecurityProtocolType.Ssl3 Or CType(192, SecurityProtocolType) Or CType(768, SecurityProtocolType)
        Dim WS
        If Not IO.File.Exists(rdrCSD("CSD").ToString.Trim.Replace("pfx", "pem")) Then
            If Not GeneraArchivos() Then
                Return False
            End If
        End If
        If rdrFactGral("GLFO_CANCELPEND") <> "Y" Then
            Console.WriteLine("Iniciando solicitud de cancelación...")
            Dim cancel
            Dim UUIDS
            Dim cancelResponse
            Dim archivoCer = IO.File.ReadAllBytes(rdrCSD("CSD").ToString.Trim.Replace("pfx", "pem"))
            Dim archivoKey

            If TestFlag Then
                WS = New CancelacionDemoFinkok.CancelSOAP
                cancel = New CancelacionDemoFinkok.cancel
                UUIDS = New CancelacionDemoFinkok.UUIDS
                cancelResponse = New CancelacionDemoFinkok.cancelResponse
                cancel.password = "&2Bdk2yA"
                archivoKey = IO.File.ReadAllBytes(rdrCSD("CSD").ToString.Trim.Replace(".pfx", "Test.enc"))
            Else
                WS = New CancelacionFinkok.CancelSOAP
                cancel = New CancelacionFinkok.cancel
                UUIDS = New CancelacionFinkok.UUIDS
                cancelResponse = New CancelacionFinkok.cancelResponse
                cancel.password = "B9h/Ww8q"
                archivoKey = IO.File.ReadAllBytes(rdrCSD("CSD").ToString.Trim.Replace("pfx", "enc"))
            End If

            Console.WriteLine("Cancelando folio fiscal: " & arrUUID(0))
            Console.WriteLine("Por favor espere...")
            UUIDS.uuids = {arrUUID(0)}
            cancel.username = "direccion@mobilemetriks.com"
            cancel.taxpayer_id = rdrEmisor("VATNUM").ToString.Trim
            cancel.cer = archivoCer

            cancel.key = archivoKey
            cancel.store_pending = False
            cancel.UUIDS = UUIDS

            Try
                'WS.cancelaCFDi("MOB100617FNA", "ewyfndkpm", WebSAF_MOB100617FNADataSet.Emisores.Rows(0).Item("RFC"), {comprobante("UUID").ToString}, Convert.FromBase64String(PFX), Password)
                cancelResponse = WS.cancel(cancel)
                If cancelResponse.cancelResult.CodEstatus = Nothing Then
                    Dim estatusUUID
                    Dim EstatusCancelacion
                    Try
                        estatusUUID = cancelResponse.cancelResult.Folios(0).EstatusUUID
                    Catch ex As Exception
                        estatusUUID = ""
                    End Try
                    Try
                        EstatusCancelacion = cancelResponse.cancelResult.Folios(0).EstatusCancelacion
                    Catch ex As Exception
                        EstatusCancelacion = ""
                    End Try
                    If estatusUUID = "no_cancelable" Then
                        Console.WriteLine("El CFDI no se puede cancelar debido a que contiene CFDI relacionados")
                        Console.WriteLine("Presiona enter para continuar...")
                        Console.Read()
                    End If
                    If estatusUUID = 201 Or estatusUUID = 202 Then
                        Dim fechaCancelacion As Date = Datos.ObtieneFecha + TimeZone.CurrentTimeZone.GetUtcOffset(Now)
                        Dim fechaInicial As New DateTime(1988, 1, 1, 0, 0, 0)
                        Dim fechaCancelacionBigInt = DateDiff(DateInterval.Day, fechaInicial, fechaCancelacion) * 1440
                        Console.WriteLine(estatusUUID & " - " & EstatusCancelacion)
                        'Console.WriteLine("UPDATE " & Empresa & ".dbo.INVOICES SET GLFO_CANCELPEND='Y', GLFO_FECHSOLCANC = '" & fechaCancelacion & "' WHERE IV=" & rdrFactGral("IV"))
                        Datos.UpdateDB("UPDATE " & Empresa & ".dbo.INVOICES SET GLFO_CANCELPEND='Y', GLFO_FECHSOLCANC = " & fechaCancelacionBigInt & "  WHERE IV=" & rdrFactGral("IV"))
                        Console.WriteLine("Presiona enter para continuar...")
                        Console.Read()
                        Return True
                    ElseIf estatusUUID = 708 Then
                        Console.WriteLine("No se pudo conectar con el SAT en este momento, favor de intentar más tarde.")
                        Console.WriteLine("Presiona enter para continuar...")
                        Console.Read()
                        Return False
                    Else
                        Console.WriteLine(estatusUUID & " - " & EstatusCancelacion & "Favor de intentar más tarde, sí el problema persiste deberá comunicarse a soporte técnico")
                        Console.Read()
                        Return False
                        '       Exit Function
                    End If
                Else
                    Console.WriteLine(cancelResponse.cancelResult.CodEstatus & vbCrLf & "Favor de intentar más tarde, sí el problema persiste deberá comunicarse a soporte técnico")
                    Console.Read()
                    Return False
                    '    Exit Function
                End If
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Console.Read()
                Return False
                'Exit Function
            End Try
        Else
            'Revisamos Cancelación
            Console.WriteLine("Revisando estado de cancelación...")
            Dim total As String = totalCFDI
            Dim RFC As String = rdrReceptor("VATNUM")
            Dim pruebaCancel
            Dim pruebaRespuesta
            If TestFlag Then
                WS = New CancelacionDemoFinkok.CancelSOAP
                pruebaCancel = New CancelacionDemoFinkok.get_sat_status
                pruebaRespuesta = New CancelacionDemoFinkok.get_sat_statusResponse
                'cancel = New CancelacionDemoFinkok.cancel
                'UUIDS = New CancelacionDemoFinkok.UUIDS
                'cancelResponse = New CancelacionDemoFinkok.cancelResponse
                pruebaCancel.password = "&2Bdk2yA"
            Else
                WS = New CancelacionFinkok.CancelSOAP
                pruebaCancel = New CancelacionFinkok.get_sat_status
                pruebaRespuesta = New CancelacionFinkok.get_sat_statusResponse
                'cancel = New CancelacionFinkok.cancel
                'UUIDS = New CancelacionFinkok.UUIDS
                'cancelResponse = New CancelacionFinkok.cancelResponse
                pruebaCancel.password = "B9h/Ww8q"
            End If

            pruebaCancel.username = "direccion@mobilemetriks.com"
            pruebaCancel.taxpayer_id = rdrEmisor("VATNUM").ToString.Trim
            pruebaCancel.rtaxpayer_id = RFC
            pruebaCancel.uuid = arrUUID(0)
            pruebaCancel.total = total
            Console.WriteLine("Folio fiscal a consultar: " & arrUUID(0))
            Try
                pruebaRespuesta = WS.Get_sat_status(pruebaCancel)
                Console.WriteLine("EL estado del CFDI es: " & pruebaRespuesta.get_sat_statusResult.sat.Estado)
                Console.Read()
                If pruebaRespuesta.get_sat_statusResult.sat.Estado = "Cancelado" Then
                    Return True
                End If
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
                Console.Read()
            End Try
            Return False
        End If

    End Function

    Public Function CancelaEDICOM(ByVal arrUUID As String()) As Boolean
        Dim wsEDICOM As New com.sedeb2b.cfdiws.CFDiService
        Dim cancelaRespuesta As com.sedeb2b.cfdiws.CancelaResponse

        Try
            Console.WriteLine("Cancelando folio fiscal: " & arrUUID(0))
            wsEDICOM.Timeout = 600000
            cancelaRespuesta = wsEDICOM.cancelaCFDi("MOB100617FNA", "ewyfndkpm", rdrEmisor("VATNUM").ToString.Trim, arrUUID, File.ReadAllBytes(rdrCSD("CSD").ToString.Trim), rdrCSD("Password").ToString.Trim)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.Read()
            Return False
        End Try

        Dim strUUIDs As String = ""
        For Each uuid As String In cancelaRespuesta.uuids
            strUUIDs &= uuid & vbCrLf
        Next

        Console.WriteLine("Se cancelaron los siguientes folios fiscales:" & vbCrLf & strUUIDs)
        Return True
    End Function

    Sub ValidationEventHandler(ByVal sender As Object, ByVal e As ValidationEventArgs)
        Select Case e.Severity
            Case XmlSeverityType.Error
                Console.WriteLine("Error validando CFDI " & vbCrLf & e.Message)
            Case XmlSeverityType.Warning
                Console.WriteLine("Advertencia validando CFDI " & vbCrLf & e.Message)
        End Select
        validSchema = False
    End Sub

    Public Function ReporteMensualCFDv2(ByVal DocXML As XmlDocument, ByVal NameSpaceCFDv2 As String, ByVal Activa As Boolean) As String
        Dim reporte As String = "|#1#|#2#|#3#|#4#|#5#|#6#|#7#|#8#|#9#|#10#|#11#|#12#|"

        ' RFC del receptor
        Dim Atributos As XmlAttributeCollection = DocXML.GetElementsByTagName("Receptor", NameSpaceCFDv2)(0).Attributes
        reporte = Replace(reporte, "#1#", Atributos("rfc").Value)

        ' Datos del comprobante
        Atributos = DocXML.GetElementsByTagName("Comprobante", NameSpaceCFDv2)(0).Attributes
        reporte = Replace(reporte, "#2#", Atributos("serie").Value)
        reporte = Replace(reporte, "#3#", Atributos("folio").Value)
        reporte = Replace(reporte, "#4#", Atributos("anoAprobacion").Value & Atributos("noAprobacion").Value)
        reporte = Replace(reporte, "#5#", Replace(Atributos("fecha").Value, "T", " "))
        reporte = Replace(reporte, "#6#", Atributos("total").Value)

        ' Datos del IVA Trasladado
        Try
            For Each NodoXML As XmlNode In DocXML.GetElementsByTagName("Traslado", NameSpaceCFDv2)
                Atributos = NodoXML.Attributes
                If Atributos("impuesto").Value = "IVA" Then
                    reporte = Replace(reporte, "#7#", Atributos("importe").Value)
                End If
            Next
        Catch ex As Exception
            reporte = Replace(reporte, "#7#", "")
        End Try

        reporte = Replace(reporte, "#8#", IIf(Activa, "1", "0"))
        ' Datos del tipo de comprobante
        Atributos = DocXML.GetElementsByTagName("Comprobante", NameSpaceCFDv2)(0).Attributes
        Select Case Atributos("tipoDeComprobante").Value
            Case "ingreso"
                reporte = Replace(reporte, "#9#", "I")
            Case "egreso"
                reporte = Replace(reporte, "#9#", "E")
            Case "traslado"
                reporte = Replace(reporte, "#9#", "T")
        End Select

        ' Datos de informacion aduanera
        Dim Pedimentos As String = ""
        Dim FechasImp As String = ""
        Dim Aduanas As String = ""
        Dim lstPedimentos As New List(Of String)
        Dim lstFechasImp As New List(Of String)
        Dim lstAduanas As New List(Of String)
        Try
            For Each NodoXML As XmlNode In DocXML.GetElementsByTagName("InformacionAduanera", NameSpaceCFDv2)
                Atributos = NodoXML.Attributes
                lstPedimentos.Add(Atributos("numero").Value)
                lstFechasImp.Add(Atributos("fecha").Value)
                lstAduanas.Add(Atributos("aduana").Value)
            Next
            Pedimentos = Join(lstPedimentos.ToArray, ",")
            FechasImp = Join(lstFechasImp.ToArray, ",")
            Aduanas = Join(lstAduanas.ToArray, ",")
        Catch ex As Exception
            Exit Try
        End Try

        reporte = Replace(reporte, "#10#", Pedimentos)
        reporte = Replace(reporte, "#11#", FechasImp)
        reporte = Replace(reporte, "#12#", Aduanas)

        Return reporte
    End Function

    Public Function ObtieneFecha() As Date
        'Dim wsLicencia As New LicenciasSAF.Service1
        Dim ds As Date

        Try
            ds = wsLicencia.obtieneFecha
            Return ds
        Catch ex As Exception
            Return Date.UtcNow
        End Try
    End Function

    Public Sub CreaDataSetCFDI3_3(ByRef dsNuevo As DataSet, ByVal EsVistaPrevia As Boolean, ByVal CadenaOriginalTFD As String, ByVal TipoComprobante As String, ByVal Activa As Boolean)
        Dim fila As System.Data.DataRow '= ds.Tables("Emisor").Rows(0)
        'Dim filaReceptor As System.Data.DataRowView = bsReceptores.Current
        Dim imagenCBB As New System.IO.MemoryStream

        If dsNuevo.Tables("InformacionAduanera") Is Nothing Then
            dsNuevo.Tables.Add("InformacionAduanera")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("numero")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("fecha")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("aduana")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("Parte_Id")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("Parte") Is Nothing Then
            dsNuevo.Tables.Add("Parte")
            dsNuevo.Tables("Parte").Columns.Add("cantidad")
            dsNuevo.Tables("Parte").Columns.Add("unidad")
            dsNuevo.Tables("Parte").Columns.Add("noIdentificacion")
            dsNuevo.Tables("Parte").Columns.Add("descripcion")
            dsNuevo.Tables("Parte").Columns.Add("valorUnitario")
            dsNuevo.Tables("Parte").Columns.Add("importe")
            dsNuevo.Tables("Parte").Columns.Add("Parte_Id")
            dsNuevo.Tables("Parte").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("ComplementoConcepto") Is Nothing Then
            dsNuevo.Tables.Add("ComplementoConcepto")
            dsNuevo.Tables("ComplementoConcepto").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("CuentaPredial") Is Nothing Then
            dsNuevo.Tables.Add("CuentaPredial")
            dsNuevo.Tables("CuentaPredial").Columns.Add("numero")
            dsNuevo.Tables("CuentaPredial").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("Impuestos") Is Nothing Then
            dsNuevo.Tables.Add("Impuestos")
            dsNuevo.Tables("Impuestos").Columns.Add("totalImpuestosRetenidos")
            dsNuevo.Tables("Impuestos").Columns.Add("totalImpuestosTrasladados")
            dsNuevo.Tables("Impuestos").Columns.Add("Impuestos_Id")
            dsNuevo.Tables("Impuestos").Columns.Add("Comprobante_Id")
        End If

        If dsNuevo.Tables("Retenciones") Is Nothing Then
            dsNuevo.Tables.Add("Retenciones")
            dsNuevo.Tables("Retenciones").Columns.Add("Impuestos_Id")
            dsNuevo.Tables("Retenciones").Columns.Add("Retenciones_Id")
            Dim filaR As DataRow = dsNuevo.Tables("Retenciones").NewRow
            filaR("Impuestos_Id") = 0
            filaR("Retenciones_Id") = 0
            dsNuevo.Tables("Retenciones").Rows.Add(filaR)
        End If

        If dsNuevo.Tables("Traslados") Is Nothing Then
            dsNuevo.Tables.Add("Traslados")
            dsNuevo.Tables("Traslados").Columns.Add("Impuestos_Id")
            dsNuevo.Tables("Traslados").Columns.Add("Traslados_Id")
        End If

        If dsNuevo.Tables("Retencion") Is Nothing Then
            dsNuevo.Tables.Add("Retencion")
            dsNuevo.Tables("Retencion").Columns.Add("impuesto")
            dsNuevo.Tables("Retencion").Columns.Add("importe")
            dsNuevo.Tables("Retencion").Columns.Add("Retenciones_Id")
            Dim filaR As DataRow = dsNuevo.Tables("Retencion").NewRow
            filaR("impuesto") = "IVA"
            filaR("importe") = "0"
            filaR("Retenciones_Id") = 0
            dsNuevo.Tables("Retencion").Rows.Add(filaR)
        End If

        If dsNuevo.Tables("Traslado") Is Nothing Then
            dsNuevo.Tables.Add("Traslado")
            dsNuevo.Tables("Traslado").Columns.Add("impuesto")
            dsNuevo.Tables("Traslado").Columns.Add("tasa")
            dsNuevo.Tables("Traslado").Columns.Add("importe")
            dsNuevo.Tables("Traslado").Columns.Add("Traslados_Id")
        End If

        dsNuevo.Tables.Add("DomicilioFiscal")
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("calle", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("noExterior", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("noInterior", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("colonia", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("municipio", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("estado", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("pais", System.Type.GetType("System.String"))
        dsNuevo.Tables("DomicilioFiscal").Columns.Add("codigoPostal", System.Type.GetType("System.String"))
        Dim filaDomicilio As System.Data.DataRow = dsNuevo.Tables("DomicilioFiscal").NewRow
        filaDomicilio("calle") = rdrEmisor("ADDRESS")
        filaDomicilio("noExterior") = rdrEmisor("GLFO_NUMERO")
        filaDomicilio("noInterior") = rdrEmisor("GLFO_MUNICIPIO")
        filaDomicilio("colonia") = rdrEmisor("GLFO_COLONIA")
        filaDomicilio("municipio") = rdrEmisor("GLFO_DELEG")
        filaDomicilio("estado") = rdrEmisor("STATENAME")
        filaDomicilio("pais") = rdrEmisor("COUNTRYNAME")
        filaDomicilio("codigoPostal") = rdrEmisor("ZIP")
        dsNuevo.Tables("domicilioFiscal").Rows.Add(filaDomicilio)

        dsNuevo.Tables.Add("Domicilio")
        dsNuevo.Tables("Domicilio").Columns.Add("calle", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("noExterior", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("noInterior", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("colonia", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("municipio", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("estado", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("pais", System.Type.GetType("System.String"))
        dsNuevo.Tables("Domicilio").Columns.Add("codigoPostal", System.Type.GetType("System.String"))
        If rdrFactGral("GLFO_FACTORAJE") <> "Y" Then
            Dim filaDom As System.Data.DataRow = dsNuevo.Tables("Domicilio").NewRow
            filaDom("calle") = rdrReceptor("ADDRESS")
            filaDom("noExterior") = rdrReceptor("GLFO_NUMERO")
            filaDom("noInterior") = rdrReceptor("GLFO_MUNICIPIO")
            filaDom("colonia") = rdrReceptor("GLFO_COLONIA")
            filaDom("municipio") = rdrReceptor("GLFO_DELEG")
            filaDom("estado") = rdrReceptor("STATENAME")
            filaDom("pais") = rdrReceptor("COUNTRYNAME")
            filaDom("codigoPostal") = rdrReceptor("ZIP")
            dsNuevo.Tables("Domicilio").Rows.Add(filaDom)
        End If
        dsNuevo.Tables.Add("Adicionales")
        dsNuevo.Tables("Adicionales").Columns.Add("Activa", System.Type.GetType("System.Boolean"))
        dsNuevo.Tables("Adicionales").Columns.Add("CBB", System.Type.GetType("System.Byte[]"))
        dsNuevo.Tables("Adicionales").Columns.Add("CadenaOriginal_TFD", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Tipo", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Logotipo", System.Type.GetType("System.Byte[]"))
        dsNuevo.Tables("Adicionales").Columns.Add("ImporteLetra", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Observaciones", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Telefono", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("UUIDRel", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("TipoRelacion", System.Type.GetType("System.String"))
        Try
            dsNuevo.Tables("Comprobante").Columns.Add("FormaPago", System.Type.GetType("System.String"))
            dsNuevo.Tables("Concepto").Columns.Add("SerieFolio", System.Type.GetType("System.String"))
            dsNuevo.Tables("Concepto").Columns.Add("UUIDPagado", System.Type.GetType("System.String"))
            dsNuevo.Tables("Concepto").Columns.Add("FechaPagado", System.Type.GetType("System.String"))
            dsNuevo.Tables("Concepto").Columns.Add("ParcialidadPagado", System.Type.GetType("System.String"))
            dsNuevo.Tables("Concepto").Columns.Add("SaldoAnterior", System.Type.GetType("System.String"))
            dsNuevo.Tables("Concepto").Columns.Add("ImportePagado", System.Type.GetType("System.String"))
        Catch ex As Exception
            Exit Try
        End Try
        fila = dsNuevo.Tables("Adicionales").NewRow

        If Not EsVistaPrevia Then
            If Not CadenaOriginalTFD.ToUpper = "CFDV2" Then
                Datos.GeneraCBB(imagenCBB, String.Format("?re={0}&rr={1}&tt={2:0000000000.000000}&id={3}", dsNuevo.Tables("Emisor").Rows(0).Item("rfc").ToString, dsNuevo.Tables("Receptor").Rows(0).Item("rfc").ToString, CDbl(dsNuevo.Tables("Comprobante").Rows(0).Item("total").ToString), dsNuevo.Tables("TimbreFiscalDigital").Rows(0).Item("UUID").ToString))
            End If
            fila("Activa") = Activa
            fila("CBB") = imagenCBB.ToArray
            fila("CadenaOriginal_TFD") = CadenaOriginalTFD
            fila("Tipo") = TipoComprobante
            If File.Exists(rdrEmisor("EXTFILENAME").ToString.Trim) Then
                fila("Logotipo") = File.ReadAllBytes(rdrEmisor("EXTFILENAME").ToString.Trim)
            Else
                fila("Logotipo") = Nothing
            End If
            fila("ImporteLetra") = ImporteConLetra(rdrFactGral("AFTERWTAX").ToString, rdrFactGral("NAME").ToString, rdrFactGral("CODE").ToString)
            fila("Observaciones") = rdrFactGral("OBSERVACIONES").ToString.Trim.Replace("|", vbCrLf)
            fila("Telefono") = rdrEmisor("PHONE").ToString.Trim
        Else
            dsNuevo.Tables.Add("Complemento")
            dsNuevo.Tables("Complemento").Columns.Add("Comprobante_Id")
            dsNuevo.Tables("Complemento").Columns.Add("Complemento_Id")

            dsNuevo.Tables.Add("TimbreFiscalDigital")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("version")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("UUID")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("FechaTimbrado")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("selloCFD")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("noCertificadoSAT")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("selloSAT")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("Complemento_Id")

            Datos.GeneraCBB(imagenCBB, String.Format("?re={0}&rr={1}&tt={2:0000000000.000000}&id={3}", dsNuevo.Tables("Emisor").Rows(0).Item("rfc").ToString, dsNuevo.Tables("Receptor").Rows(0).Item("rfc").ToString, CDbl(dsNuevo.Tables("Comprobante").Rows(0).Item("total").ToString), "487e16be-ec40-450e-aa1a-1a2d8495209e"))
            fila("Activa") = Activa
            fila("CBB") = imagenCBB.ToArray
            fila("CadenaOriginal_TFD") = CadenaOriginalTFD
            fila("Tipo") = TipoComprobante
            If File.Exists(rdrEmisor("EXTFILENAME").ToString.Trim) Then
                fila("Logotipo") = File.ReadAllBytes(rdrEmisor("EXTFILENAME").ToString.Trim)
            Else
                fila("Logotipo") = Nothing
            End If

            fila("ImporteLetra") = ImporteConLetra(rdrFactGral("AFTERWTAX").ToString, rdrFactGral("NAME").ToString, rdrFactGral("CODE").ToString)
            fila("Observaciones") = rdrFactGral("OBSERVACIONES").ToString.Trim.Replace("|", vbCrLf)
            fila("Telefono") = rdrEmisor("PHONE").ToString.Trim


        End If
        Dim totalRecibo = 0.0
        Dim monto As Double = 0
        If reciboPago Then
            TipoComprobante = "Recibo de Pago"
            Console.WriteLine("ReciboPago")
            Try
                Dim indice1 As Integer = 0
                Dim indice2 As Integer = 0
                Dim credito(99) As Double
                For Each factura As DataRow In rdrFactPagadas.Rows
                    If factura("CREDIT1") < 0 Then
                        credito(factura("GLFO_RELAC")) += factura("CREDIT1") * -1
                    Else
                        totalRecibo += factura("CREDIT2")
                    End If
                Next
                Dim tipoCambio As Double = Math.Round(rdrFactGral("AFTERWTAX") / totalRecibo, 2)
                'Console.WriteLine(credito)
                'For Each filaConcepto As DataRow In dsNuevo.Tables("Concepto").Rows
                For Each filafactura As DataRow In rdrFactPagadas.Rows
                    If filafactura("CREDIT1") > 0 Then
                        'Dim tipoCambio As Double = 1.0
                        Dim importePagado As Double = 0
                        If Not rdrFactGral("CODE").ToString = rdrFactPagadas(indice1)("CODE") Then
                            tipoCambio = Math.Round(rdrFactPagadas(indice1)("IVBALANCE2") / rdrFactPagadas(indice1)("IVBALANCE"), 4)
                            importePagado = Math.Round(rdrFactPagadas(indice1)("CREDIT1") / tipoCambio, 2)
                            If (filafactura("GLFO_RELAC") <> 0) Then importePagado -= Math.Round(credito(filafactura("GLFO_RELAC")) / tipoCambio, 2)
                            monto += importePagado * tipoCambio
                        Else
                            If rdrFactPagadas(indice1)("CODE") = "MXN" Then
                                importePagado = rdrFactPagadas(indice1)("CREDIT1")
                            Else
                                importePagado = rdrFactPagadas(indice1)("CREDIT2")
                            End If
                            If (filafactura("GLFO_RELAC") <> 0) Then importePagado -= credito(filafactura("GLFO_RELAC"))
                            monto += importePagado
                        End If
                        Dim parcialidad = rdrFactPagadas(indice1)("PARCIALIDAD").ToString
                        If parcialidad = 0 Then parcialidad = 1
                        'Dim saldoAnterior As Double = importePagado - Math.Round((rdrFactPagadas(indice1)("IVBALANCE2") / tipoCambio), 2)
                        Dim saldoAnterior As Double = (rdrFactPagadas(indice1)("IVBALANCE") * -1)
                        If (filafactura("GLFO_RELAC") <> 0) Then saldoAnterior -= credito(filafactura("GLFO_RELAC"))
                        Dim saldoInsoluto As Double = saldoAnterior - importePagado
                        'Dim saldoInsoluto As Double = Math.Round((rdrFactPagadas(indice1)("IVBALANCE2") / tipoCambio) * -1, 2)
                        'Dim saldoAnterior As Double = saldoInsoluto + importePagado
                        dsNuevo.Tables("Concepto")(indice2)("SerieFolio") = rdrFactPagadas(indice1)("IVNUMFACT").ToString
                        dsNuevo.Tables("Concepto")(indice2)("UUIDPagado") = rdrFactPagadas(indice1)("GLOB_FOLIOFISCAL").ToString
                        dsNuevo.Tables("Concepto")(indice2)("FechaPagado") = Format(Convert.ToDateTime(rdrFactGral("GLFO_FECPAGO").ToString), "yyyy-MM-ddTHH:mm:ss")
                        dsNuevo.Tables("Concepto")(indice2)("ParcialidadPagado") = parcialidad
                        dsNuevo.Tables("Concepto")(indice2)("SaldoAnterior") = saldoAnterior
                        dsNuevo.Tables("Concepto")(indice2)("ImportePagado") = importePagado
                        dsNuevo.Tables("Concepto")(indice2)("Importe") = tipoCambio
                        indice2 += 1
                        'Exit For
                    End If
                    indice1 += 1
                Next
                'Next

                For Each comp As DataRow In dsNuevo.Tables("Comprobante").Rows
                    TotalFactura = monto
                    comp("Total") = monto
                    comp("Moneda") = rdrFactGral("CODE").ToString
                    comp("FormaPago") = rdrFactGral("GLOB_FORMAPAGO").ToString.Substring(0, 2)
                Next
                fila("Tipo") = "Recibo de Pago"

                fila("ImporteLetra") = ImporteConLetra(monto, rdrFactGral("NAME").ToString, rdrFactGral("CODE").ToString)
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
                Console.Read()
            End Try
        End If
        If rdrFactGral("GLFO_SUSTITUYE") = "Y" Then
            fila("UUIDRel") = rdrFactRel("GLOB_FOLIOFISCAL")
            fila("TipoRelacion") = rdrFactGral("GLFO_TRELACION")
        End If

        dsNuevo.Tables("Adicionales").Rows.Add(fila)
        'dsNuevo.Tables("Comprobante").Rows(0).Item("metodoDePago") = rdrReceptor("GLFO_CONDPAGO").ToString
    End Sub

    Public Sub CreaDataSetCFDI(ByRef dsNuevo As DataSet, ByVal EsVistaPrevia As Boolean, ByVal CadenaOriginalTFD As String, ByVal TipoComprobante As String, ByVal Activa As Boolean)
        Dim fila As System.Data.DataRow '= ds.Tables("Emisor").Rows(0)
        'Dim filaReceptor As System.Data.DataRowView = bsReceptores.Current
        Dim imagenCBB As New System.IO.MemoryStream

        If dsNuevo.Tables("InformacionAduanera") Is Nothing Then
            dsNuevo.Tables.Add("InformacionAduanera")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("numero")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("fecha")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("aduana")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("Parte_Id")
            dsNuevo.Tables("InformacionAduanera").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("Parte") Is Nothing Then
            dsNuevo.Tables.Add("Parte")
            dsNuevo.Tables("Parte").Columns.Add("cantidad")
            dsNuevo.Tables("Parte").Columns.Add("unidad")
            dsNuevo.Tables("Parte").Columns.Add("noIdentificacion")
            dsNuevo.Tables("Parte").Columns.Add("descripcion")
            dsNuevo.Tables("Parte").Columns.Add("valorUnitario")
            dsNuevo.Tables("Parte").Columns.Add("importe")
            dsNuevo.Tables("Parte").Columns.Add("Parte_Id")
            dsNuevo.Tables("Parte").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("ComplementoConcepto") Is Nothing Then
            dsNuevo.Tables.Add("ComplementoConcepto")
            dsNuevo.Tables("ComplementoConcepto").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("CuentaPredial") Is Nothing Then
            dsNuevo.Tables.Add("CuentaPredial")
            dsNuevo.Tables("CuentaPredial").Columns.Add("numero")
            dsNuevo.Tables("CuentaPredial").Columns.Add("Concepto_Id")
        End If

        If dsNuevo.Tables("Impuestos") Is Nothing Then
            dsNuevo.Tables.Add("Impuestos")
            dsNuevo.Tables("Impuestos").Columns.Add("totalImpuestosRetenidos")
            dsNuevo.Tables("Impuestos").Columns.Add("totalImpuestosTrasladados")
            dsNuevo.Tables("Impuestos").Columns.Add("Impuestos_Id")
            dsNuevo.Tables("Impuestos").Columns.Add("Comprobante_Id")
        End If

        If dsNuevo.Tables("Retenciones") Is Nothing Then
            dsNuevo.Tables.Add("Retenciones")
            dsNuevo.Tables("Retenciones").Columns.Add("Impuestos_Id")
            dsNuevo.Tables("Retenciones").Columns.Add("Retenciones_Id")
            Dim filaR As DataRow = dsNuevo.Tables("Retenciones").NewRow
            filaR("Impuestos_Id") = 0
            filaR("Retenciones_Id") = 0
            dsNuevo.Tables("Retenciones").Rows.Add(filaR)
        End If

        If dsNuevo.Tables("Traslados") Is Nothing Then
            dsNuevo.Tables.Add("Traslados")
            dsNuevo.Tables("Traslados").Columns.Add("Impuestos_Id")
            dsNuevo.Tables("Traslados").Columns.Add("Traslados_Id")
        End If

        If dsNuevo.Tables("Retencion") Is Nothing Then
            dsNuevo.Tables.Add("Retencion")
            dsNuevo.Tables("Retencion").Columns.Add("impuesto")
            dsNuevo.Tables("Retencion").Columns.Add("importe")
            dsNuevo.Tables("Retencion").Columns.Add("Retenciones_Id")
            Dim filaR As DataRow = dsNuevo.Tables("Retencion").NewRow
            filaR("impuesto") = "IVA"
            filaR("importe") = "0"
            filaR("Retenciones_Id") = 0
            dsNuevo.Tables("Retencion").Rows.Add(filaR)
        End If

        If dsNuevo.Tables("Traslado") Is Nothing Then
            dsNuevo.Tables.Add("Traslado")
            dsNuevo.Tables("Traslado").Columns.Add("impuesto")
            dsNuevo.Tables("Traslado").Columns.Add("tasa")
            dsNuevo.Tables("Traslado").Columns.Add("importe")
            dsNuevo.Tables("Traslado").Columns.Add("Traslados_Id")
        End If

        If dsNuevo.Tables("domicilioFiscal").Columns("calle") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("calle")
        If dsNuevo.Tables("domicilioFiscal").Columns("noExterior") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("noExterior")
        If dsNuevo.Tables("domicilioFiscal").Columns("noInterior") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("noInterior")
        If dsNuevo.Tables("domicilioFiscal").Columns("colonia") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("colonia")
        If dsNuevo.Tables("domicilioFiscal").Columns("municipio") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("municipio")
        If dsNuevo.Tables("domicilioFiscal").Columns("estado") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("estado")
        If dsNuevo.Tables("domicilioFiscal").Columns("pais") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("pais")
        If dsNuevo.Tables("domicilioFiscal").Columns("codigoPostal") Is Nothing Then dsNuevo.Tables("domicilioFiscal").Columns.Add("codigoPostal")
        If dsNuevo.Tables("domicilioFiscal").Rows(0)("noExterior") Is System.DBNull.Value Or dsNuevo.Tables("domicilioFiscal").Rows(0)("noExterior") Is String.Empty Or dsNuevo.Tables("domicilioFiscal").Rows(0)("noExterior").ToString.Trim = "" Then
            dsNuevo.Tables("domicilioFiscal").Rows(0)("noExterior") = "."
        End If
        If dsNuevo.Tables("domicilioFiscal").Rows(0)("noInterior") Is System.DBNull.Value Or dsNuevo.Tables("domicilioFiscal").Rows(0)("noInterior") Is String.Empty Or dsNuevo.Tables("domicilioFiscal").Rows(0)("noInterior").ToString.Trim = "" Then
            dsNuevo.Tables("domicilioFiscal").Rows(0)("noInterior") = "."
        End If

        If dsNuevo.Tables("domicilio").Columns("calle") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("calle")
        If dsNuevo.Tables("domicilio").Columns("noExterior") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("noExterior")
        If dsNuevo.Tables("domicilio").Columns("noInterior") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("noInterior")
        If dsNuevo.Tables("domicilio").Columns("colonia") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("colonia")
        If dsNuevo.Tables("domicilio").Columns("municipio") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("municipio")
        If dsNuevo.Tables("domicilio").Columns("estado") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("estado")
        If dsNuevo.Tables("domicilio").Columns("pais") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("pais")
        If dsNuevo.Tables("domicilio").Columns("codigoPostal") Is Nothing Then dsNuevo.Tables("domicilio").Columns.Add("codigoPostal")

        dsNuevo.Tables.Add("Adicionales")
        dsNuevo.Tables("Adicionales").Columns.Add("Activa", System.Type.GetType("System.Boolean"))
        dsNuevo.Tables("Adicionales").Columns.Add("CBB", System.Type.GetType("System.Byte[]"))
        dsNuevo.Tables("Adicionales").Columns.Add("CadenaOriginal_TFD", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Tipo", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Logotipo", System.Type.GetType("System.Byte[]"))
        dsNuevo.Tables("Adicionales").Columns.Add("ImporteLetra", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Observaciones", System.Type.GetType("System.String"))
        dsNuevo.Tables("Adicionales").Columns.Add("Telefono", System.Type.GetType("System.String"))
        

        fila = dsNuevo.Tables("Adicionales").NewRow

        If Not EsVistaPrevia Then
            If Not CadenaOriginalTFD.ToUpper = "CFDV2" Then
                Datos.GeneraCBB(imagenCBB, String.Format("?re={0}&rr={1}&tt={2:0000000000.000000}&id={3}", dsNuevo.Tables("Emisor").Rows(0).Item("rfc").ToString, dsNuevo.Tables("Receptor").Rows(0).Item("rfc").ToString, CDbl(dsNuevo.Tables("Comprobante").Rows(0).Item("total").ToString), dsNuevo.Tables("TimbreFiscalDigital").Rows(0).Item("UUID").ToString))
            End If
            fila("Activa") = Activa
            fila("CBB") = imagenCBB.ToArray
            fila("CadenaOriginal_TFD") = CadenaOriginalTFD
            fila("Tipo") = TipoComprobante
            If File.Exists(rdrEmisor("EXTFILENAME").ToString.Trim) Then
                fila("Logotipo") = File.ReadAllBytes(rdrEmisor("EXTFILENAME").ToString.Trim)
            Else
                fila("Logotipo") = Nothing
            End If
            fila("ImporteLetra") = ImporteConLetra(rdrFactGral("AFTERWTAX").ToString, rdrFactGral("NAME").ToString, rdrFactGral("CODE").ToString)
            fila("Observaciones") = rdrFactGral("OBSERVACIONES").ToString.Trim.Replace("|", vbCrLf)
            fila("Telefono") = rdrEmisor("PHONE").ToString.Trim
        Else
            dsNuevo.Tables.Add("Complemento")
            dsNuevo.Tables("Complemento").Columns.Add("Comprobante_Id")
            dsNuevo.Tables("Complemento").Columns.Add("Complemento_Id")

            dsNuevo.Tables.Add("TimbreFiscalDigital")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("version")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("UUID")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("FechaTimbrado")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("selloCFD")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("noCertificadoSAT")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("selloSAT")
            dsNuevo.Tables("TimbreFiscalDigital").Columns.Add("Complemento_Id")

            Datos.GeneraCBB(imagenCBB, String.Format("?re={0}&rr={1}&tt={2:0000000000.000000}&id={3}", dsNuevo.Tables("Emisor").Rows(0).Item("rfc").ToString, dsNuevo.Tables("Receptor").Rows(0).Item("rfc").ToString, CDbl(dsNuevo.Tables("Comprobante").Rows(0).Item("total").ToString), "487e16be-ec40-450e-aa1a-1a2d8495209e"))
            fila("Activa") = Activa
            fila("CBB") = imagenCBB.ToArray
            fila("CadenaOriginal_TFD") = CadenaOriginalTFD
            fila("Tipo") = TipoComprobante
            If File.Exists(rdrEmisor("EXTFILENAME").ToString.Trim) Then
                fila("Logotipo") = File.ReadAllBytes(rdrEmisor("EXTFILENAME").ToString.Trim)
            Else
                fila("Logotipo") = Nothing
            End If

            fila("ImporteLetra") = ImporteConLetra(rdrFactGral("AFTERWTAX").ToString, rdrFactGral("NAME").ToString, rdrFactGral("CODE").ToString)
            fila("Observaciones") = rdrFactGral("OBSERVACIONES").ToString.Trim.Replace("|", vbCrLf)
            fila("Telefono") = rdrEmisor("PHONE").ToString.Trim
        End If
        
        dsNuevo.Tables("Adicionales").Rows.Add(fila)
        dsNuevo.Tables("Comprobante").Rows(0).Item("metodoDePago") = rdrReceptor("GLFO_CONDPAGO").ToString
    End Sub

    Public Sub GeneraCBB(ByRef CBB As System.IO.MemoryStream, ByVal datos As String)
        Dim bc As New BarcodeLib.Barcode.QRCode.QRCode

        'bc.Data = "?re=XAXX010101000&rr=XAXX010101000&tt=1234567890.123456&id=ad662d33-6934-459c-a128-BDf0393f0f44"
        'bc.Data = "?re=MOB100617FNA&rr=CAA970530UQ2&tt=1234567890.123456&id=ad662d33-6934-459c-a128-BDf0393f0f44"
        'bc.Data = "http://www.mobilemetriks.com"
        bc.Data = datos
        bc.ModuleSize = 7
        bc.LeftMargin = 0
        bc.RightMargin = 0
        bc.TopMargin = 0
        bc.BottomMargin = 0
        bc.Resolution = 800
        bc.Encoding = BarcodeLib.Barcode.QRCode.QRCodeEncoding.Auto
        bc.Version = BarcodeLib.Barcode.QRCode.QRCodeVersion.Auto
        bc.ECL = BarcodeLib.Barcode.QRCode.ErrorCorrectionLevel.L

        Dim imagen As New System.Drawing.Bitmap(258, 259)
        Dim graimagen As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(imagen)
        bc.drawBarcode(graimagen)

        For y As Integer = 0 To 199
            For x As Integer = 0 To 199
                If imagen.GetPixel(x, y).Name = "ffff9c48" Or imagen.GetPixel(x, y).Name = "ffff489c" Then
                    GeneraCBB(CBB, datos)
                    Exit Sub
                End If
            Next
        Next

        'picCBB.Image = imagen

        '
        'Dim ms As New System.IO.MemoryStream
        imagen.Save(CBB, System.Drawing.Imaging.ImageFormat.Png)
        'picCBB.Image = New System.Drawing.Bitmap(ms)
    End Sub

    Public Function GeneraArchivos() As Boolean

        Dim A As Boolean = False
        Dim RutaCSD = rdrCSD("CSD").ToString.Trim.Replace("pfx", "cer")
        Dim RutaPEM = rdrCSD("CSD").ToString.Trim.Replace("pfx", "pem")
        Dim RutaKEYPEM = rdrCSD("CSD").ToString.Trim.Replace(".pfx", "key.pem")
        Dim RutaENC = rdrCSD("CSD").ToString.Trim.Replace("pfx", "enc")
        Dim RutaKey = rdrCSD("RutaKey").ToString.Trim
        Dim password = rdrCSD("Password").ToString.Trim
        If Not IO.File.Exists(rdrCSD("CSD").ToString.Trim.Replace("pfx", "cer")) Then
            Console.WriteLine("No se encontró el Certificado Digital. Favor de comunicarse con soporte técnico.")
            Console.WriteLine("Presiona enter o cierra la ventana para continuar...")
            Console.Read()
            Return False
        End If
        Dim proceso4 As New Process()
        proceso4.StartInfo.FileName = "SSL\openssl.exe"
        'proceso4.StartInfo.Arguments = "x509 -inform der -in """ & RutaCSD & """ -out ""temps\archivo.crt"""
        proceso4.StartInfo.Arguments = "x509 -inform DER -outform PEM -in """ & RutaCSD & """ -pubkey -out """ & RutaPEM & """"
        proceso4.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        proceso4.StartInfo.CreateNoWindow = False
        proceso4.Start()
        proceso4.WaitForExit(100)
        While Not proceso4.HasExited
        End While
        proceso4.Close()
        proceso4 = Nothing
        'Process.Start("SSL\openssl.exe x509 -inform DER -outform PEM -in """ & RutaCSD & """ -pubkey -out """ & RutaPEM & """")
        'esperamos a que termine el proceso
        While Not A
            Dim rd As IO.StreamReader
            Try
                rd = New IO.StreamReader(RutaPEM)
                rd.ReadLine()
                A = True
                rd.Close()
            Catch ex As Exception
                'rd.Close()
            End Try
        End While
        'Console.WriteLine("3")
        'PASO -5-
        'se crea un proceso que gerenra un archivo .pem a partir del archivo .key
        'esto es debido a que el .key está en formato binario (DER) y hay que convertirlo a base64 PEM (para que tenga los encabezados)
        Console.WriteLine("Archivo PEM creado")
        'Console.Read()
        A = False
        'Process.Start("SSL\openssl.exe pkcs8 -inform DER -in """ & RutaKey & """ -passin pass:" & password & " -out " & RutaKEYPEM & """")
        Dim proceso5 As New Process()
        proceso5.StartInfo.FileName = "SSL\openssl.exe"
        proceso5.StartInfo.Arguments = "pkcs8 -inform DER -in """ & RutaKey & """ -passin pass:" & password & " -out """ & RutaKEYPEM & """"
        proceso5.StartInfo.WindowStyle = ProcessWindowStyle.Maximized
        proceso5.StartInfo.CreateNoWindow = False
        proceso5.Start()
        proceso5.WaitForExit(1000)
        While Not proceso5.HasExited
        End While
        'Dim error1 As String = proceso5.StandardOutput.ReadToEnd
        'Console.WriteLine(error1)
        'Dim y As Integer = proceso1.ExitCode
        proceso5.Close()
        proceso5 = Nothing

        'esperamos a que termine el proceso
        'While Not A
        '    Dim rd As IO.StreamReader
        '    Try
        '        rd = New IO.StreamReader(RutaKEYPEM)
        '        rd.ReadLine()
        '        A = True
        '        rd.Close()
        '    Catch ex As Exception
        '        'rd.Close()
        '    End Try
        'End While
        'Console.WriteLine("4")
        'PASO -6-
        Console.WriteLine("Archivo KEYPEM creado")
        'Console.Read()
        'se crea un proceso que gerenra un archivo .pfx a partir del archivo .pem y del archivo .crt los cuales están convertidos a base 64
        Dim passwordFinkok As String
        A = False
        If TestFlag Then
            passwordFinkok = "&2Bdk2yA"
        Else
            passwordFinkok = "B9h/Ww8q"
        End If
        'Process.Start("SSL\openssl.exe rsa -in """ & RutaKEYPEM & """ -des3 -out """ & RutaENC & """ -passout pass:" & passwordFinkok)
        Dim proceso6 As New Process()
        proceso6.StartInfo.FileName = "SSL\openssl.exe"
        'proceso6.StartInfo.Arguments = "pkcs12 -export -out ""temps\archivo.pfx"" -inkey ""temps\archivo.pem"" -in ""temps\archivo.crt"" -passin pass:" & PasswordTextBox.Text & " -password pass:" & PasswordTextBox.Text
        proceso6.StartInfo.Arguments = "rsa -in """ & RutaKEYPEM & """ -des3 -out """ & RutaENC & """ -passout pass:" & passwordFinkok
        proceso6.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        proceso6.StartInfo.CreateNoWindow = True
        proceso6.Start()
        proceso6.WaitForExit(2000)
        While Not proceso6.HasExited
        End While
        'Dim x As Integer = proceso2.ExitCode
        proceso6.Close()
        proceso6 = Nothing

        'esperamos a que termine el proceso
        While Not A
            Dim rd As IO.StreamReader
            Try
                rd = New IO.StreamReader(RutaENC)
                rd.ReadLine()
                A = True
                rd.Close()
            Catch ex As Exception
                'rd.Close()
            End Try
        End While
        Console.WriteLine("Archivo ENC creado")
        'Console.Read()
        Return True
    End Function

End Class

Public Class FirmaParams
    Dim _ArchivoXSD As String
    Dim _ArchivoXSLT As String
    Dim _EspacioNombres As String
    Dim _NodoCert As String
    Dim _AtrSello As String
    Dim _AtrCertif As String
    Dim _AtrNoCert As String

    Public Sub New()

    End Sub

    Public Property ArchivoXSD As String
        Get
            Return _ArchivoXSD
        End Get
        Set(value As String)
            _ArchivoXSD = value
        End Set
    End Property

    Public Property ArchivoXSLT As String
        Get
            Return _ArchivoXSLT
        End Get
        Set(value As String)
            _ArchivoXSLT = value
        End Set
    End Property

    Public Property EspacioNombres As String
        Get
            Return _EspacioNombres
        End Get
        Set(value As String)
            _EspacioNombres = value
        End Set
    End Property

    Public Property NodoCert As String
        Get
            Return _NodoCert
        End Get
        Set(value As String)
            _NodoCert = value
        End Set
    End Property

    Public Property AtrSello As String
        Get
            Return _AtrSello
        End Get
        Set(value As String)
            _AtrSello = value
        End Set
    End Property

    Public Property AtrCertif As String
        Get
            Return _AtrCertif
        End Get
        Set(value As String)
            _AtrCertif = value
        End Set
    End Property

    Public Property AtrNoCert As String
        Get
            Return _AtrNoCert
        End Get
        Set(value As String)
            _AtrNoCert = value
        End Set
    End Property
End Class