Imports System.IO
Imports System.Text
Imports System.Xml

Module moduleB2S

    Public Const AppTitle As String = "B2S Designer"

    Public Const MaxBulbIntensity As Integer = 5

    Public DefaultEMReels As String() = New String() {"EMR_T1_0", "EMR_T2_0", "EMR_T3_0", "EMR_T4_0", "EMR_T5_0", "EMR_T6_0"}
    Public DefaultEMCreditReels As String() = New String() {"EMR_CT1_00", "EMR_CT2_00", "EMR_CT3_00"}
    Public DefaultLEDs As String() = New String() {"LED_0", "LED_Blue_0"}
    Public ImportedStartString As String = "Imported"

    Public ImageFileExtensionFilter As String = My.Resources.TXT_AllImages & "|*.png; *.jpg; *.jpeg; *.gif; *.bmp|PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|BITMAP (*.bmp)|*.bmp|ALL (*.*)|*.*"

    Private EXEDir As String = Application.StartupPath
    Private Const ProjectDir As String = "Projects"

    Public ReadOnly Property BackglassProjectsPath() As String
        Get
            Return IO.Path.Combine(EXEDir, ProjectDir)
        End Get
    End Property
    Public ReadOnly Property ProjectPath() As String
        Get
            Return IO.Path.Combine(BackglassProjectsPath, Backglass.currentData.Name)
        End Get
    End Property
    Public ReadOnly Property ProjectImagesPath() As String
        Get
            Return IO.Path.Combine(ProjectPath, "My Resources")
        End Get
    End Property
    Public ReadOnly Property SettingsFileName As String
        Get
            Return IO.Path.Combine(BackglassProjectsPath, "B2SBackglassDesigner.Settings.xml")
        End Get
    End Property
    Public ReadOnly Property ImportFileName() As String
        Get
            Return IO.Path.Combine(BackglassProjectsPath, "B2SBackglassDesigner.Import.xml")
        End Get
    End Property

    Public XmlSettings As Xml.XmlDocument = Nothing

    Public Property CurrentB2S() As String = String.Empty

    Public Property DefaultLightColor() As Color = Color.FromArgb(&HFF, &HFF, &HFE, &HE8)

    Public Property DefaultOpacity() As Single = 1
    Public Property DefaultVPTablesFolder() As String = String.Empty
    Public Property LatestAuthor() As String = String.Empty
    Public Property LatestImportDirectory() As String = String.Empty

    Public Property NoToolEvents() As Boolean = False

    Public ReadOnly Property Headline() As String
        Get
            Return If(Not String.IsNullOrEmpty(CurrentB2S), CurrentB2S & " - ", "") & AppTitle
        End Get
    End Property

    Public Sub UpdateStatusBar(form As formDesigner, tab As B2STabPage)
        If tab IsNot Nothing Then
            ' update zoom factor
            form.tscmbZoomInPercent.Text = tab.BackglassData.Zoom.ToString() & "%"
        End If
        ' update file info
        form.tsLabelFileInfo.Padding = New Padding(10, 0, 10, 0)
        If tab IsNot Nothing AndAlso tab.Image IsNot Nothing AndAlso Not String.IsNullOrEmpty(tab.BackglassData.ImageFileName) Then
            form.tsLabelFileInfo.BorderSides = ToolStripStatusLabelBorderSides.Left
            form.tsLabelFileInfo.ImageAlign = ContentAlignment.MiddleRight
            form.tsLabelFileInfo.Image = My.Resources.chooseback3
            form.tsLabelFileInfo.TextAlign = ContentAlignment.MiddleCenter
            form.tsLabelFileInfo.Text = tab.BackglassData.ImageFileName
        Else
            form.tsLabelFileInfo.BorderSides = ToolStripStatusLabelBorderSides.None
            form.tsLabelFileInfo.Image = Nothing
            form.tsLabelFileInfo.Text = String.Empty
        End If
        ' update file size info
        form.tsLabelFileSize.Padding = New Padding(10, 0, 10, 0)
        If tab IsNot Nothing AndAlso tab.Image IsNot Nothing Then
            form.tsLabelFileSize.BorderSides = ToolStripStatusLabelBorderSides.Left
            form.tsLabelFileSize.ImageAlign = ContentAlignment.MiddleRight
            form.tsLabelFileSize.Image = My.Resources.imagesize
            form.tsLabelFileSize.TextAlign = ContentAlignment.MiddleCenter
            If Backglass.currentData.IsDMDImageShown Then
                If tab.DMDImage IsNot Nothing Then
                    form.tsLabelFileSize.Text = tab.DMDImage.Width.ToString() & " x " & tab.DMDImage.Height.ToString()
                Else
                    form.tsLabelFileSize.Text = String.Empty
                End If
            Else
                form.tsLabelFileSize.Text = tab.Image.Width.ToString() & " x " & tab.Image.Height.ToString()
            End If
        Else
            form.tsLabelFileSize.BorderSides = ToolStripStatusLabelBorderSides.None
            form.tsLabelFileSize.Image = Nothing
            form.tsLabelFileSize.Text = String.Empty
        End If
    End Sub

    Public Sub UpdateStatusBar4Mouse(form As formDesigner, tab As B2STabPage, loc As Point)
        form.tsLabelMarker.Padding = New Padding(10, 0, 10, 0)
        If tab IsNot Nothing AndAlso tab.Image IsNot Nothing Then
            form.tsLabelMarker.BorderSides = ToolStripStatusLabelBorderSides.Left
            form.tsLabelMarker.ImageAlign = ContentAlignment.MiddleRight
            form.tsLabelMarker.Image = My.Resources.imagemarker
            form.tsLabelMarker.TextAlign = ContentAlignment.MiddleCenter
            form.tsLabelMarker.Text = loc.X.ToString() & ", " & loc.Y.ToString()
        Else
            form.tsLabelMarker.BorderSides = ToolStripStatusLabelBorderSides.None
            form.tsLabelMarker.Image = Nothing
            form.tsLabelMarker.Text = String.Empty
        End If
    End Sub

    Public Sub FillReelListView(ByVal type As eImageSetType,
                                ByVal ilReelsAndLEDs As ImageList,
                                ByVal lvReelsAndLEDs As ListView,
                                ByVal ReelLEDList As String(),
                                Optional ByVal clearList As Boolean = False,
                                Optional ByVal showselected As Boolean = False,
                                Optional ByVal selected As String = "")

        If clearList Then
            lvReelsAndLEDs.Items.Clear()
            ilReelsAndLEDs.Images.Clear()
        End If

        ' set list view's image list
        lvReelsAndLEDs.LargeImageList = ilReelsAndLEDs
        lvReelsAndLEDs.SmallImageList = ilReelsAndLEDs

        ' get images of reels and leds into the image list
        Dim i As Integer = ilReelsAndLEDs.Images.Count
        For Each item As String In ReelLEDList
            If Not showselected OrElse selected.Contains("," & item & ",") Then
                ilReelsAndLEDs.Images.Add(item, My.Resources.ResourceManager.GetObject(item))
                lvReelsAndLEDs.Items.Add(item, If(item.Substring(0, 3) = "LED", My.Resources.ReelsAndLEDs_TypeLED, If(item.Substring(0, 5) = "EMR_C", My.Resources.ReelsAndLEDs_TypeEMCreditReel, My.Resources.ReelsAndLEDs_TypeEMReel)) & If(Not showselected, " " & (i + 1).ToString(), ""), i)
                i += 1
            End If
        Next

        Dim imagesets As GeneralData.Data.ImageSetCollection = Nothing
        Dim key As String = String.Empty
        Dim name As String = My.Resources.ReelsAndLEDs_TypeImported & " "
        Select Case type
            Case eImageSetType.ReelImages
                imagesets = GeneralData.currentData.ImportedReelImageSets
                key = ImportedStartString & "EMR_T{0}_0"
                name &= My.Resources.ReelsAndLEDs_TypeEMReel
            Case eImageSetType.CreditReelImages
                imagesets = GeneralData.currentData.ImportedCreditReelImageSets
                key = ImportedStartString & "EMR_CT{0}_00"
                name &= My.Resources.ReelsAndLEDs_TypeEMCreditReel
            Case eImageSetType.LEDImages
                imagesets = GeneralData.currentData.ImportedLEDImageSets
                key = ImportedStartString & "LED_T{0}_0"
                name &= My.Resources.ReelsAndLEDs_TypeLED
        End Select
        If imagesets IsNot Nothing Then
            For Each imageset As KeyValuePair(Of Integer, Image()) In imagesets
                Dim item As String = String.Format(key, imageset.Key.ToString())
                If Not showselected OrElse selected.Contains("," & item & ",") Then
                    ilReelsAndLEDs.Images.Add(item, imageset.Value(0))
                    lvReelsAndLEDs.Items.Add(item, name & " " & (i + 1).ToString(), i)
                    i += 1
                End If
            Next
        End If

    End Sub

    Public Function GetRenderedLEDName(ByVal reel As String) As String
        Dim ret As String = String.Empty
        reel = reel.Replace("RenderedLED0", "RenderedLED").Replace("Dream7LED0", "Dream7LED")
        If reel.StartsWith("RenderedLED", StringComparison.CurrentCultureIgnoreCase) Then
            ret = reel
        ElseIf reel.StartsWith("Dream7LED", StringComparison.CurrentCultureIgnoreCase) Then
            ret = "RenderedLED" & reel.Substring(9)
        End If
        Return ret
    End Function
    Public Function IsReelImageRendered(ByVal reel As String) As Boolean
        Return reel.StartsWith("RenderedLED", StringComparison.CurrentCultureIgnoreCase)
    End Function
    Public Function IsReelImageDream7(ByVal reel As String) As Boolean
        Return reel.StartsWith("Dream7", StringComparison.CurrentCultureIgnoreCase)
    End Function
    Public Function GetReelImage(ByVal reeltype As String,
                                 ByVal reelcolor As Color,
                                 Optional ByVal dream7 As Boolean = False,
                                 Optional ByVal d7thickness As Single = 1,
                                 Optional ByVal d7shear As Single = 1,
                                 Optional ByVal d7glow As Single = 1,
                                 Optional ByVal newsize As Size = Nothing) As Image
        If (String.IsNullOrEmpty(reeltype) OrElse reeltype.Equals("0") OrElse reeltype.Equals("1") OrElse reeltype.Equals("2")) AndAlso Not String.IsNullOrEmpty(Backglass.currentData.ReelType) Then
            reeltype = Backglass.currentData.ReelType
        End If
        If IsReelImageRendered(reeltype) Then
            Static image As Image = Nothing
            Static type As String = String.Empty
            Static color As Color = Nothing
            Static d7 As Boolean = False
            'If Not type.Equals(reeltype.Substring(11)) OrElse Not reelcolor.Equals(color) OrElse Not dream7.Equals(d7) Then
            type = reeltype.Substring(11)
            color = reelcolor
            d7 = dream7
            If Not dream7 Then
                Dim led As B2SRenderedLED = New B2SRenderedLED
                Select Case type
                    Case "7", "8"
                        led.LEDType = B2SRenderedLED.eLEDType.LED8
                    Case "9", "10"
                        led.LEDType = B2SRenderedLED.eLEDType.LED10
                    Case "14"
                        led.LEDType = B2SRenderedLED.eLEDType.LED14
                    Case "16"
                        led.LEDType = B2SRenderedLED.eLEDType.LED16
                End Select
                image = led.Image(reelcolor)
            Else
                Dim led As Dream7Display = New Dream7Display
                led.Size = newsize
                If led.Width <= 0 OrElse led.Height <= 0 Then led.Size = New Size(90, 120)
                led.Digits = 1
                Select Case type
                    Case "7", "8"
                        led.Type = SegmentNumberType.SevenSegment
                    Case "9", "10"
                        led.Type = SegmentNumberType.TenSegment
                    Case "14"
                        led.Type = SegmentNumberType.FourteenSegment
                    Case "16"
                        'led.Type = 
                End Select
                led.SetValue(0, 65535)
                led.ScaleMode = ScaleMode.Stretch
                'led.ForeColor = reelcolor
                If Not reelcolor.Equals(color.FromArgb(0, 0, 0)) Then
                    led.LightColor = reelcolor
                    led.GlassColor = color.FromArgb(Math.Min(reelcolor.R + 50, 255), Math.Min(reelcolor.G + 50, 255), Math.Min(reelcolor.B + 50, 255))
                    led.GlassColorCenter = color.FromArgb(Math.Min(reelcolor.R + 70, 255), Math.Min(reelcolor.G + 70, 255), Math.Min(reelcolor.B + 70, 255))
                Else
                    led.LightColor = color.FromArgb(15, 15, 15)
                    led.GlassColor = color.FromArgb(15, 15, 15)
                    led.GlassColorCenter = color.FromArgb(15, 15, 15)
                End If
                led.OffColor = color.FromArgb(15, 15, 15)
                'led.BackColor = Color.FromArgb(5, 5, 5)
                led.BackColor = color.FromArgb(15, 15, 15)
                led.GlassAlpha = 140
                led.GlassAlphaCenter = 255
                'led.Thickness = d7thickness * 1.2
                led.Shear = d7shear
                led.Glow = d7glow
                Try
                    Dim bmp As Bitmap = New Bitmap(led.Width, led.Height)
                    led.DrawToBitmap(bmp, New Rectangle(New Point(0, 0), led.Size))
                    'bmp.Save("c:\tmp\bmp.png")
                    image = bmp
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
            End If
            'End If
            Return image
        ElseIf reeltype.StartsWith(ImportedStartString) Then
            Try
                Select Case reeltype.Substring(8, 5)
                    Case "EMR_T" : Return GeneralData.currentData.ImportedReelImageSets(CInt(reeltype.Substring(13).Replace("_0", "")))(0)
                    Case "EMR_CT" : Return GeneralData.currentData.ImportedCreditReelImageSets(CInt(reeltype.Substring(14).Replace("_0", "")))(0)
                    Case "LED_T" : Return GeneralData.currentData.ImportedLEDImageSets(CInt(reeltype.Substring(13).Replace("_0", "")))(0)
                    Case Else : Return Nothing
                End Select
            Catch
                Return Nothing
            End Try
        Else
            Return My.Resources.ResourceManager.GetObject(If(reeltype.EndsWith("0"), reeltype, reeltype & "_0"))
        End If
    End Function
    Public Function GetDream7LEDType(ByVal type As String) As String
        Select Case type
            Case "7", "8"
                Return "SevenSegment"
            Case "9", "10"
                Return "TenSegment"
            Case "14"
                Return "FourteenSegment"
            Case "16"
                Return "SixteenSegment"
            Case Else
                Return "SevenSegment"
        End Select
    End Function
    Public Function GetFirstReelType() As String
        Dim ret As String = String.Empty
        For Each reeltype As String In Backglass.currentData.ReelType.Split(",")
            If Not String.IsNullOrEmpty(reeltype) Then
                ret = reeltype
                Exit For
            End If
        Next
        Return ret
    End Function

    Public Function CompareImages(ByRef image1 As Image, ByRef image2 As Image) As Boolean
        Dim equal As Boolean = True
        If image1.Height = image2.Height AndAlso image1.Width = image2.Width Then
            Dim rect As Rectangle = New Rectangle(0, 0, image1.Width, image1.Height)
            Dim image1Data As System.Drawing.Imaging.BitmapData = DirectCast(image1, System.Drawing.Bitmap).LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image1.PixelFormat)
            Dim image2Data As System.Drawing.Imaging.BitmapData = DirectCast(image2, System.Drawing.Bitmap).LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image2.PixelFormat)
            Dim image1ByteCount As Integer = Math.Abs(image1Data.Stride) * image1.Height
            Dim image2ByteCount As Integer = Math.Abs(image2Data.Stride) * image2.Height
            If image1ByteCount = image2ByteCount Then
                Dim image1RGB(image1ByteCount) As Byte
                Dim image2RGB(image2ByteCount) As Byte
                System.Runtime.InteropServices.Marshal.Copy(image1Data.Scan0, image1RGB, 0, image1ByteCount)
                System.Runtime.InteropServices.Marshal.Copy(image2Data.Scan0, image2RGB, 0, image2ByteCount)
                For i As Integer = 0 To (image1ByteCount - 1)
                    If Not image1RGB(i) = image2RGB(i) Then
                        equal = False
                        Exit For
                    End If
                Next
            End If
            DirectCast(image1, System.Drawing.Bitmap).UnlockBits(image1Data)
            DirectCast(image2, System.Drawing.Bitmap).UnlockBits(image2Data)
        End If

        Return equal
    End Function

    Public Function TrimImage(ByRef _image As Image) As Rectangle
        Dim rect As Rectangle = New Rectangle(0, 0, _image.Width, _image.Height)
        Dim imageData As System.Drawing.Imaging.BitmapData = DirectCast(_image, System.Drawing.Bitmap).LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
        Dim imageByteCount As Integer = Math.Abs(imageData.Stride) * imageData.Height
        Dim imageARGB(imageByteCount) As Byte
        System.Runtime.InteropServices.Marshal.Copy(imageData.Scan0, imageARGB, 0, imageByteCount)
        DirectCast(_image, System.Drawing.Bitmap).UnlockBits(imageData)

        Dim left As Integer = imageData.Width
        Dim right As Integer = 0
        Dim top As Integer = imageData.Height
        Dim bottom As Integer = 0
        For y As Integer = 0 To (imageData.Height - 1)
            For x As Integer = 0 To (imageData.Width - 1)
                Dim isNotTransparent As Boolean = imageARGB((((y * imageData.Width) + x) * 4) + 3) > 0
                If isNotTransparent Then
                    If left > x Then left = x
                    If right < x Then right = x
                    If top > y Then top = y
                    If bottom < y Then bottom = y
                End If
            Next
        Next
        If left > right Or top > bottom Then
            Return Rectangle.Empty ' Fully transparent
        End If
        Return New Rectangle(left, top, right - left + 1, bottom - top + 1)
    End Function

    Public Function ImageToBase64(image As Image) As String
        If image IsNot Nothing Then
            With New System.Drawing.ImageConverter
                Dim bytes() As Byte = CType(.ConvertTo(image, GetType(Byte())), Byte())
                Return Convert.ToBase64String(bytes, Base64FormattingOptions.None)
            End With
        Else
            Return String.Empty
        End If
    End Function
    Public Function Base64ToImage(data As String) As Image
        Dim image As Image = Nothing
        If data.Length > 0 Then
            Dim bytes() As Byte = Convert.FromBase64String(data)
            If bytes IsNot Nothing AndAlso bytes.Length > 0 Then
                With New System.Drawing.ImageConverter
                    image = CType(.ConvertFrom(bytes), Image)
                End With
            End If
        End If
        Return image
    End Function

    Public Function WavToBase64(stream As IO.Stream) As String
        If stream IsNot Nothing Then
            Dim bytes() As Byte
            ReDim bytes(stream.Length - 1)
            Using reader As IO.BinaryReader = New IO.BinaryReader(stream)
                Dim length As Integer = reader.Read(bytes, 0, stream.Length)
            End Using
            Return Convert.ToBase64String(bytes, Base64FormattingOptions.None)
        Else
            Return String.Empty
        End If
    End Function
    Public Function Base64ToWav(data As String) As IO.Stream
        If data.Length > 0 Then
            Dim bytes() As Byte = Convert.FromBase64String(data)
            Return New IO.MemoryStream(bytes)
        Else
            Return Nothing
        End If
    End Function

    Public Function Color2String(ByVal color As Color) As String
        Return color.R.ToString() & "." & color.G.ToString() & "." & color.B.ToString()
    End Function
    Public Function String2Color(ByVal color As String) As Color
        Dim colorvalues As String() = color.Split(".")
        Return Drawing.Color.FromArgb(CInt(colorvalues(0)), CInt(colorvalues(1)), CInt(colorvalues(2)))
    End Function

    Public Function SafeParseInt(_value As String, _defaultValue As Integer) As Integer
        Dim result As Integer
        If Integer.TryParse(_value, result) Then
            Return result
        End If
        Return _defaultValue
    End Function
    Public Function SafeParseBool(_value As String, _defaultValue As Boolean) As Boolean
        Dim result As Integer
        If Integer.TryParse(_value, result) Then
            Return result = 1
        End If
        Return _defaultValue
    End Function

    Public Function TranslateIndex2DodgeColor(ByVal index As Integer) As Color
        Select Case index
            Case 1 : Return Color.Red
            Case 2 : Return Color.FromArgb(0, 255, 0)
            Case 3 : Return Color.Blue
            Case 4 : Return Color.Yellow
            Case 5 : Return Color.FromArgb(0, 255, 255)
            Case 6 : Return Color.FromArgb(255, 0, 255)
            Case 7 : Return Color.White
            Case Else : Return Nothing
        End Select
    End Function
    Public Function TranslateDodgeColor2Index(ByVal col As Color) As Integer
        If col.Equals(Color.FromArgb(255, 0, 0)) OrElse col.Equals(Color.Red) Then
            Return 1
        ElseIf col.Equals(Color.FromArgb(0, 255, 0)) OrElse col.Equals(Color.Green) Then
            Return 2
        ElseIf col.Equals(Color.FromArgb(0, 0, 255)) OrElse col.Equals(Color.Blue) Then
            Return 3
        ElseIf col.Equals(Color.FromArgb(255, 255, 0)) OrElse col.Equals(Color.Yellow) Then
            Return 4
        ElseIf col.Equals(Color.FromArgb(0, 255, 255)) OrElse col.Equals(Color.Magenta) Then
            Return 5
        ElseIf col.Equals(Color.FromArgb(255, 0, 255)) Then
            Return 6
        ElseIf col.Equals(Color.FromArgb(255, 255, 255)) OrElse col.Equals(Color.White) Then
            Return 7
        Else
            Return 0
        End If
    End Function

    Public Function Secured(ByVal text As String) As String
        Dim sb As StringBuilder = New StringBuilder()
        text = text.Replace(" ", "")
        For Each letter As String In text.ToCharArray()
            Dim a As Integer = Asc(letter)
            If (a < Asc("a") OrElse a > Asc("z")) AndAlso (a < Asc("A") OrElse a > Asc("Z")) AndAlso (a < Asc("0") OrElse a > Asc("9")) Then
                letter = "_"
            End If
            sb.Append(letter)
        Next
        Return sb.ToString()
    End Function

    Public Function IsFontInstalled(fontName As String) As Boolean
        Using testFont = New Font(fontName, 8)
            Return 0 = String.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase)
        End Using
    End Function

    Public Function IsOverlappingB2SStartDigit() As Boolean
        Dim ret As Boolean = False
        Dim usedDigits As Generic.List(Of Integer) = New Generic.List(Of Integer)
        For Each score As ReelAndLED.ScoreInfo In Backglass.currentScores
            If score.B2SStartDigit > 0 Then
                For i As Integer = 0 To score.Digits - 1
                    If usedDigits.Contains(score.B2SStartDigit + i) Then
                        ret = True
                        Exit For
                    End If
                    usedDigits.Add(score.B2SStartDigit + i)
                Next
            End If
        Next
        Return ret
    End Function

    Public Function IsThereAlreadyOneSelfRotatingSnippit(ByVal bulbid As Integer) As Boolean
        Dim ret As Boolean = False
        For Each illu As Illumination.BulbInfo In Backglass.currentBulbs
            If illu.IsImageSnippit AndAlso illu.ID <> bulbid AndAlso illu.SnippitInfo.SnippitType = eSnippitType.SelfRotatingImage Then
                ret = True
                Exit For
            End If
        Next
        Return ret
    End Function

    Public Function ExportSetImages(_selectedName As String, _selectedIndex As Integer, _xmlPath As String, _defaults As String(), _nodeName As String) As Boolean
        Try
            ' Check if the selected is part of defaults
            If _selectedIndex < _defaults.Length Then
                ' Export hardcoded images
                Dim baseName As String = _defaults(_selectedIndex)
                Dim outputDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports", _selectedName)

                If Not Directory.Exists(outputDirectory) Then
                    Directory.CreateDirectory(outputDirectory)
                End If

                Dim success As Boolean = False
                Dim index As Integer = 0
                For index = 0 To 9999 ' Loop through images 0 to 9999
                    Dim resourceName As String
                    If baseName.Contains("_00") Then ' EMR_CT1_00
                        resourceName = $"{baseName.Replace("_00", "")}_{index:D2}"
                    Else ' EMR_T1_0 or LED_T1_0
                        resourceName = $"{baseName.Replace("_0", "")}_{index}"
                    End If

                    Dim image As Object = My.Resources.ResourceManager.GetObject(resourceName)

                    If image IsNot Nothing AndAlso TypeOf image Is Image Then
                        Dim outputFileName As String = Path.Combine(outputDirectory, $"{_selectedName.Replace(" ", "_")}_{index:D2}.png")
                        outputFileName.Replace(" ", "_")
                        CType(image, Image).Save(outputFileName, Imaging.ImageFormat.Png)
                        success = True
                    Else
                        Exit For
                    End If
                Next

                If success Then
                    MessageBox.Show($"Successfully exported {index} images! Files saved to: {outputDirectory}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return True
                Else
                    MessageBox.Show($"No images found for: {_selectedName}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If
            Else
                ' Handle imported reels (XML-based)
                Dim doc As New XmlDocument()
                doc.Load(_xmlPath)

                ' Locate the node
                Dim node As XmlNode = doc.SelectSingleNode(_nodeName)
                If node Is Nothing Then
                    MessageBox.Show("{_nodeName} node Not found in the XML file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If

                ' Collect imported sets and adjust the index
                Dim setNodes As XmlNodeList = node.ChildNodes
                Dim adjustedIndex As Integer = _selectedIndex - _defaults.Length

                If adjustedIndex < 0 OrElse adjustedIndex >= setNodes.Count Then
                    MessageBox.Show($"No data found for: {_selectedName}. Adjusted Index: {adjustedIndex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If

                ' Export images from XML
                Dim matchingNode As XmlNode = setNodes(adjustedIndex)
                Dim outputDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports", _selectedName)

                If Not Directory.Exists(outputDirectory) Then
                    Directory.CreateDirectory(outputDirectory)
                End If

                Dim index As Integer = 1
                For Each imageNode As XmlNode In matchingNode.ChildNodes
                    Dim base64Value As String = imageNode.Attributes("Value")?.Value
                    If Not String.IsNullOrEmpty(base64Value) Then
                        Dim imageBytes As Byte() = Convert.FromBase64String(base64Value)
                        Dim outputFileName As String = Path.Combine(outputDirectory, $"{_selectedName.Replace(" ", "_")}_{index:D2}.png")
                        File.WriteAllBytes(outputFileName, imageBytes)
                        index += 1
                    End If
                Next

                MessageBox.Show($"Successfully exported {index} images! Files saved to: {outputDirectory}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return True
            End If
        Catch ex As Exception
            MessageBox.Show($"Error exporting images: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Public Function ExportReelImages(_selectedName As String, _selectedIndex As Integer, _xmlPath As String) As Boolean
        Return ExportSetImages(_selectedName, _selectedIndex, _xmlPath, DefaultEMReels, "//ReelSets")
    End Function

    Public Function ExportCreditReelImages(_selectedName As String, _selectedIndex As Integer, _xmlPath As String) As Boolean
        Return ExportSetImages(_selectedName, _selectedIndex, _xmlPath, DefaultEMCreditReels, "//CreditReelSets")
    End Function

    Public Function ExportLEDImages(_selectedName As String, _selectedIndex As Integer, _xmlPath As String) As Boolean
        Return ExportSetImages(_selectedName, _selectedIndex, _xmlPath, DefaultLEDs, "//LEDSets")
    End Function

    Public Function ImportAnimations(_filePath As String) As List(Of Animation.AnimationHeader)
        Dim importedAnimations As New List(Of Animation.AnimationHeader)

        ' Load and validate the XML file
        Dim xmlDoc As New Xml.XmlDocument()
        xmlDoc.Load(_filePath)

        ' Validate the root element
        If xmlDoc.DocumentElement Is Nothing OrElse xmlDoc.DocumentElement.Name <> "Animations" Then
            Throw New Exception("The file does not have a valid <Animations> root element.")
        End If

        ' Iterate through each <Animation> node
        For Each animationNode As Xml.XmlNode In xmlDoc.DocumentElement.SelectNodes("Animation")
            Try
                Dim animation As New Animation.AnimationHeader()

                ' Safely read attributes
                animation.Name = If(animationNode.Attributes("Name")?.Value, String.Empty)
                animation.DualMode = SafeParseInt(animationNode.Attributes("DualMode")?.Value, 0)
                animation.Interval = SafeParseInt(animationNode.Attributes("Interval")?.Value, 0)
                animation.Loops = SafeParseInt(animationNode.Attributes("Loops")?.Value, 0)
                animation.IDJoin = If(animationNode.Attributes("IDJoin")?.Value, String.Empty)
                animation.StartAnimationAtBackglassStartup = SafeParseBool(animationNode.Attributes("StartAnimationAtBackglassStartup")?.Value, False)
                animation.LightsStateAtAnimationStart = SafeParseInt(animationNode.Attributes("LightsStateAtAnimationStart")?.Value, 0)
                animation.LightsStateAtAnimationEnd = SafeParseInt(animationNode.Attributes("LightsStateAtAnimationEnd")?.Value, 0)
                animation.AnimationStopBehaviour = SafeParseInt(animationNode.Attributes("AnimationStopBehaviour")?.Value, 0)
                animation.LockInvolvedLamps = SafeParseBool(animationNode.Attributes("LockInvolvedLamps")?.Value, False)
                animation.HideScoreDisplays = SafeParseBool(animationNode.Attributes("HideScoreDisplays")?.Value, False)
                animation.BringToFront = SafeParseBool(animationNode.Attributes("BringToFront")?.Value, False)
                animation.RandomStart = SafeParseBool(animationNode.Attributes("RandomStart")?.Value, False)
                animation.RandomQuality = SafeParseInt(animationNode.Attributes("RandomQuality")?.Value, 1)

                ' Read animation steps
                For Each stepNode As Xml.XmlNode In animationNode.SelectNodes("AnimationStep")
                    Dim stepItem As New Animation.AnimationStep()
                    stepItem.Step = SafeParseInt(stepNode.Attributes("Step")?.Value, 0)
                    stepItem.On = If(stepNode.Attributes("On")?.Value, String.Empty)
                    stepItem.WaitLoopsAfterOn = SafeParseInt(stepNode.Attributes("WaitLoopsAfterOn")?.Value, 0)
                    stepItem.Off = If(stepNode.Attributes("Off")?.Value, String.Empty)
                    stepItem.WaitLoopsAfterOff = SafeParseInt(stepNode.Attributes("WaitLoopsAfterOff")?.Value, 0)
                    animation.AnimationSteps.Add(stepItem)
                Next

                ' Add the animation to the list
                importedAnimations.Add(animation)
            Catch ex As Exception
                ' Log the issue for debugging
                MessageBox.Show($"Error parsing animation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next

        Return importedAnimations
    End Function

    Public Sub ExportAnimation(_animationHeader As Animation.AnimationHeader, _filePath As String)
        Dim xmlDoc As New Xml.XmlDocument()
        Dim animationsNode As Xml.XmlElement = xmlDoc.CreateElement("Animations")
        xmlDoc.AppendChild(animationsNode)

        ' Create the Animation node with attributes
        Dim animationNode As Xml.XmlElement = xmlDoc.CreateElement("Animation")
        animationNode.SetAttribute("Name", _animationHeader.Name)
        animationNode.SetAttribute("DualMode", CType(_animationHeader.DualMode, Integer).ToString()) ' Ensure numeric value is saved
        animationNode.SetAttribute("Interval", _animationHeader.Interval.ToString())
        animationNode.SetAttribute("Loops", _animationHeader.Loops.ToString())
        animationNode.SetAttribute("IDJoin", If(String.IsNullOrEmpty(_animationHeader.IDJoin), "", _animationHeader.IDJoin))
        animationNode.SetAttribute("StartAnimationAtBackglassStartup", If(_animationHeader.StartAnimationAtBackglassStartup, "1", "0"))
        animationNode.SetAttribute("LightsStateAtAnimationStart", CType(_animationHeader.LightsStateAtAnimationStart, Integer).ToString())
        animationNode.SetAttribute("LightsStateAtAnimationEnd", CType(_animationHeader.LightsStateAtAnimationEnd, Integer).ToString())
        animationNode.SetAttribute("AnimationStopBehaviour", CType(_animationHeader.AnimationStopBehaviour, Integer).ToString())
        animationNode.SetAttribute("LockInvolvedLamps", If(_animationHeader.LockInvolvedLamps, "1", "0"))
        animationNode.SetAttribute("HideScoreDisplays", If(_animationHeader.HideScoreDisplays, "1", "0"))
        animationNode.SetAttribute("BringToFront", If(_animationHeader.BringToFront, "1", "0"))
        animationNode.SetAttribute("RandomStart", If(_animationHeader.RandomStart, "1", "0"))
        animationNode.SetAttribute("RandomQuality", CType(_animationHeader.RandomQuality, Integer).ToString())

        ' Add Animation Steps as child nodes
        For Each stepItem As Animation.AnimationStep In _animationHeader.AnimationSteps
            Dim stepNode As Xml.XmlElement = xmlDoc.CreateElement("AnimationStep")
            stepNode.SetAttribute("Step", stepItem.Step.ToString())
            stepNode.SetAttribute("On", stepItem.On)
            stepNode.SetAttribute("WaitLoopsAfterOn", stepItem.WaitLoopsAfterOn.ToString())
            stepNode.SetAttribute("Off", stepItem.Off)
            stepNode.SetAttribute("WaitLoopsAfterOff", stepItem.WaitLoopsAfterOff.ToString())
            animationNode.AppendChild(stepNode)
        Next

        ' Append the animation node to the XML structure
        animationsNode.AppendChild(animationNode)

        ' Save the XML to the specified file path
        xmlDoc.Save(_filePath)
    End Sub
End Module
