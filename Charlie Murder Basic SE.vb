Imports PackageIO
Imports System

Public Class Charlie_Murder_Basic_SE

    Dim FilePath As String
    Dim filename As String
    Dim money, sp, followers, readerStart As Integer

    Public Sub Form1_load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        FilePath = OpenFile()

        If Not FilePath = Nothing Then
            Try
                ReadFile(FilePath)
            Catch ex As Exception

                MsgBox("Error Reading File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
            End Try
        End If

    End Sub

    Function OpenFile() As String

        Dim open As New OpenFileDialog

        open.Title = "Open Your Save"
        open.Filter = "(*game.sav*)|*game.sav*"
        open.FileName = "game.sav"
        filename = open.FileName

        If open.ShowDialog = Windows.Forms.DialogResult.OK Then

            saveButton.Enabled = True
            Me.MinimumSize = Me.MaximumSize : Me.Size = Me.MaximumSize
            RadCollapsiblePanel1.Enabled = True
            RadCollapsiblePanel1.IsExpanded = True

            Return open.FileName

        ElseIf open.ShowDialog = Windows.Forms.DialogResult.Cancel Then

            saveButton.Enabled = False

        Else

        End If
        Return Nothing
    End Function

    Private Sub saveButton_Click(sender As System.Object, e As System.EventArgs) Handles saveButton.Click

        MsgBox("Data Saved!")

        Try
            WriteFile(FilePath)

            Dim filePathNew As String = (FilePath + ".bak")
            System.IO.File.Delete(filePathNew)
            System.IO.File.Copy(FilePath, filePathNew)

            ReadFile(FilePath)
        Catch ex As Exception

            MsgBox("Error Writing File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
        End Try
    End Sub

    Public Sub ReadFile(ByVal FilePath As String)

        Dim Reader As New PackageIO.Reader(FilePath, PackageIO.Endian.Little)

        Dim sane, street, compare8, compare32 As Integer

        Dim blockFound, gapFound, gap2Found, offsetFound As Boolean

        blockFound = False : gapFound = False : gap2Found = False : offsetFound = False

        readerStart = 0

        Try

            sane = (Reader.SearchString("sanestats", StringType.ASCII, PackageIO.Endian.Big)(0))
            street = (Reader.SearchString("street", StringType.ASCII, PackageIO.Endian.Big)(0))

            While blockFound.Equals(False)

                If (street <= (sane + 140)) Then

                    blockFound = True

                    Reader.Position = (street - 3)

                    While gapFound.Equals(False)

                        compare8 = Reader.ReadInt8

                        Reader.Position = (Reader.Position - 1)

                        compare32 = Reader.ReadUInt32

                        Reader.Position = (Reader.Position - 3)

                        If ((compare8 = compare32) Or (compare32 = 255)) Then

                            Reader.Position = (Reader.Position + 3)

                            gapFound = True

                            While gap2Found.Equals(False)

                                compare8 = Reader.ReadUInt8

                                Reader.Position = (Reader.Position - 1)

                                compare32 = Reader.ReadUInt32

                                Reader.Position = (Reader.Position - 3)

                                If (compare8 = compare32) Then

                                    gap2Found = True

                                    Reader.Position = (Reader.Position + 7)

                                    readerStart = Reader.Position

                                    money = (Reader.ReadUInt32)

                                    Debug.Print(money)

                                    offsetFound = True

                                    If (money = 999999999) Then

                                        'WINNING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                                        moneyMaxed.ToggleState = True

                                        Reader.Position = (Reader.Position - 8)

                                        followers = Reader.ReadUInt32

                                        Reader.Position = (Reader.Position - 8)

                                        sp = Reader.ReadUInt32

                                    End If

                                    If (followers = 999999999) Then

                                        followersMaxed.ToggleState = True

                                    End If


                                    If (sp = 255) Then

                                        spMaxed.ToggleState = True

                                    End If

                                ElseIf (compare8 <> compare32) Then

                                    Reader.Position = (Reader.Position + compare8)

                                End If

                            End While

                        ElseIf (compare8 <> compare32) Then

                            Reader.Position = (Reader.Position + compare8)

                        End If
                    End While

                ElseIf (compare8 <> compare32) Then

                    Reader.Position = (Reader.Position + compare8)

                Else

                    sane = (Reader.SearchString("sanestats", StringType.ASCII, PackageIO.Endian.Big, (sane + 9))(0))

                End If
            End While

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Reader.Close()
    End Sub

    Public Sub WriteFile(ByVal FilePath As String)

        Dim Writer As New PackageIO.IO(FilePath, PackageIO.Endian.Little)

        readerStart -= 8

        Writer.Position = (readerStart)

        If (spMaxed.IsChecked = True) Then

            Writer.WriteUInt32(255)

        ElseIf (spMaxed.IsChecked = False) Then

            Writer.Position = (readerStart + 4)

        End If

        If (followersMaxed.IsChecked = True) Then

            Writer.WriteUInt32(999999999)

        ElseIf (followersMaxed.IsChecked = False) Then

            Writer.Position = (readerStart + 4)

        End If

        If (moneyMaxed.IsChecked = True) Then

            Writer.WriteUInt32(999999999)

        ElseIf (moneyMaxed.IsChecked = False) Then

            Writer.Position = (readerStart + 4)

        End If
    End Sub

    Private Sub aboutButton_Click(sender As System.Object, e As System.EventArgs) Handles aboutButton.Click
        MsgBox("Editor made by Cleric for 360Haven.com", MsgBoxStyle.Information, "About")
    End Sub

    Private Sub moneyMaxed_ToggleStateChanged(sender As System.Object, args As Telerik.WinControls.UI.StateChangedEventArgs) Handles moneyMaxed.ToggleStateChanged

        moneyMaxed.IsChecked = True
    End Sub

    Private Sub closeButton_Click(sender As System.Object, e As System.EventArgs) Handles closeButton.Click
        Me.EndInit()
        End
    End Sub

    Private Sub spMaxed_ToggleStateChanged(sender As System.Object, args As Telerik.WinControls.UI.StateChangedEventArgs) Handles spMaxed.ToggleStateChanged

        spMaxed.IsChecked = True
    End Sub

    Private Sub flowersMaxed_ToggleStateChanged(sender As System.Object, args As Telerik.WinControls.UI.StateChangedEventArgs) Handles followersMaxed.ToggleStateChanged

        followersMaxed.IsChecked = True
    End Sub
End Class
