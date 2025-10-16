Imports System

Public Class formSnippitSettings

    Public ignoreChange As Boolean = False

    Private IsDirty As Boolean = False
    Private bulbID As Integer = -1

    Public Shadows Function ShowDialog(ByVal owner As IWin32Window,
                                       ByVal id As Integer,
                                       ByRef name As String,
                                       ByRef snippittype As eSnippitType,
                                       ByRef zorder As Integer,
                                       ByRef mechid As Integer,
                                       ByRef rotatingsteps As Integer,
                                       ByRef rotatinginterval As Integer,
                                       ByRef rotatingdirection As eSnippitRotationDirection,
                                       ByRef rotationstopping As eSnippitRotationStopBehaviour) As DialogResult

        ignoreChange = True
        ' set starting values
        bulbID = id
        txtName.Text = name
        cmbType.SelectedIndex = snippittype
        If zorder < 0 Or zorder > 9 Then zorder = 0
        numericZOrder.Value = zorder
        If mechid < numericMechID.Minimum OrElse mechid > numericMechID.Maximum Then mechid = numericMechID.Minimum
        numericMechID.Value = mechid
        If rotatingsteps < numericRotatingSteps.Minimum Then rotatingsteps = numericRotatingSteps.Minimum
        If rotatingsteps > numericRotatingSteps.Maximum Then rotatingsteps = numericRotatingSteps.Maximum
        numericRotatingSteps.Value = rotatingsteps
        If rotatinginterval < numericRotatingInterval.Minimum Then rotatinginterval = numericRotatingInterval.Minimum
        If rotatinginterval > numericRotatingInterval.Maximum Then rotatinginterval = numericRotatingInterval.Maximum
        numericRotatingInterval.Value = rotatinginterval
        If rotatingdirection < eSnippitRotationDirection.Clockwise OrElse rotatingdirection > eSnippitRotationDirection.AntiClockwise Then rotatingdirection = eSnippitRotationDirection.Clockwise
        cmbRotatingDirection.SelectedIndex = rotatingdirection
        If rotationstopping < eSnippitRotationStopBehaviour.SpinOff OrElse rotationstopping > eSnippitRotationStopBehaviour.RunAnimationToFirstStep Then rotationstopping = eSnippitRotationStopBehaviour.SpinOff
        cmbRotationStopBehaviour.SelectedIndex = rotationstopping
        ignoreChange = False
        EnableDisable()
        
        ' open dialog
        Dim ret As DialogResult = MyBase.ShowDialog(owner)
        If ret = Windows.Forms.DialogResult.OK Then
            ' return some values
            name = txtName.Text
            snippittype = cmbType.SelectedIndex
            zorder = numericZOrder.Value
            mechid = numericMechID.Value
            rotatingsteps = numericRotatingSteps.Value
            rotatinginterval = numericRotatingInterval.Value
            rotatingdirection = cmbRotatingDirection.SelectedIndex
            rotationstopping = cmbRotationStopBehaviour.SelectedIndex
        End If

        Return ret

    End Function

    Private Sub Ok_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        If String.IsNullOrEmpty(txtName.Text) OrElse String.IsNullOrEmpty(cmbType.Text) Then
            MessageBox.Show(My.Resources.MSG_EnterSnippitName, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            txtName.Focus()
            Return
        End If
        MyBase.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If IsDirty Then
            Dim ret As DialogResult = MessageBox.Show(My.Resources.MSG_IsDirty, AppTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If ret = Windows.Forms.DialogResult.Yes Then
                btnOk.PerformClick()
            ElseIf ret = Windows.Forms.DialogResult.No Then
                Me.Close()
            End If
        Else
            Me.Close()
        End If
    End Sub

    Private currentType As eSnippitType = eSnippitType.StandardImage
    Private Sub Type_Enter(sender As Object, e As System.EventArgs) Handles cmbType.Enter
        currentType = cmbType.SelectedIndex
    End Sub
    Private Sub Type_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbType.SelectedIndexChanged
        If ignoreChange Then Return
        If cmbType.SelectedIndex = 1 Then
            If IsThereAlreadyOneSelfRotatingSnippit(bulbID) Then
                MessageBox.Show(My.Resources.MSG_ThereIsAlreadyOneSelfRotatingSnippit, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                If currentType <> eSnippitType.SelfRotatingImage Then
                    cmbType.SelectedIndex = currentType
                Else
                    cmbType.SelectedIndex = eSnippitType.StandardImage
                End If
            End If
        End If
        EnableDisable()
    End Sub

    Private Sub formSnippitSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        IsDirty = False
    End Sub
    Private Sub formSnippitSettings_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        IsDirty = False
        txtName.Focus()
    End Sub

    Private Sub EnableDisable()
        numericMechID.Enabled = (cmbType.SelectedIndex = 2)
        numericRotatingSteps.Enabled = (cmbType.SelectedIndex <> 0)
        numericRotatingInterval.Enabled = (cmbType.SelectedIndex = 1)
        cmbRotatingDirection.Enabled = (cmbType.SelectedIndex <> 0)
        cmbRotationStopBehaviour.Enabled = (cmbType.SelectedIndex = 1)
    End Sub

    Private Sub btnChangeImage_Click(sender As Object, e As EventArgs) Handles btnChangeImage.Click
        Dim bulb = Backglass.currentTabPage.Mouse.SelectedBulb
        If bulb IsNot Nothing Then
            Using fileDialog As OpenFileDialog = New OpenFileDialog
                With fileDialog
                    .Filter = ImageFileExtensionFilter
                    .FileName = String.Empty
                    .InitialDirectory = BackglassProjectsPath
                    If .ShowDialog(Me) = DialogResult.OK Then
                        Backglass.currentImages.RemoveByTypeAndName(Images.eImageInfoType.IlluminationSnippits, bulb.Name)

                        bulb.Image = Bitmap.FromFile(.FileName).Copy(True)
                        bulb.Name = IO.Path.GetFileNameWithoutExtension(.FileName)
                        bulb.Size.Width = bulb.Image.Width
                        bulb.Size.Height = bulb.Image.Height

                        Dim imageInfo As Images.ImageInfo = New Images.ImageInfo(Images.eImageInfoType.IlluminationSnippits)
                        imageInfo.Text = bulb.Name
                        imageInfo.Image = bulb.Image
                        Backglass.currentImages.Insert(Images.eImageInfoType.Title4IlluminationSnippits, imageInfo)

                        B2SBackglassDesigner.formDesigner.RefreshImageInfoList()
                    End If
                End With
            End Using
        End If
    End Sub

    Private Sub btnTrimImage_Click(sender As Object, e As EventArgs) Handles btnTrimImage.Click
        Dim selected_bulb = Backglass.currentTabPage.Mouse.SelectedBulb
        If selected_bulb IsNot Nothing AndAlso selected_bulb.Image IsNot Nothing Then
            Dim trim_rect As Rectangle = TrimImage(selected_bulb.Image)

            If trim_rect.X > 0 Or trim_rect.Y > 0 Or trim_rect.Width < selected_bulb.Image.Width Or trim_rect.Height < selected_bulb.Image.Height Then
                Dim trimmed As New Bitmap(trim_rect.Width, trim_rect.Height, selected_bulb.Image.PixelFormat)
                Graphics.FromImage(trimmed).DrawImage(selected_bulb.Image, New Rectangle(0, 0, trimmed.Width, trimmed.Height), trim_rect, System.Drawing.GraphicsUnit.Pixel)

                For Each bulb As Illumination.BulbInfo In Backglass.currentBulbs
                    If bulb.Name = selected_bulb.Name Then
                        bulb.Image = DirectCast(trimmed, Image)
                        bulb.Size.Width = bulb.Image.Width
                        bulb.Size.Height = bulb.Image.Height
                        bulb.Location += trim_rect.Location
                    End If
                Next

                Backglass.currentImages.SetNewImage(Images.eImageInfoType.IlluminationSnippits, selected_bulb.Name, selected_bulb.Image)
                B2SBackglassDesigner.formDesigner.RefreshImageInfoList()
            End If
        End If
    End Sub

End Class