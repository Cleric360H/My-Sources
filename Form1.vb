Imports PackageIO
Imports System.Text
Imports System.IO

Public Class deblob2

    Dim FilePath As String

    'Open File Dialog
    Function OpenFile() As String
        ToolStripStatusLabel1.Text = "Opening..."
        ProgressBar2.Visible = True
        Dim open As New OpenFileDialog
        open.Title = "Open Your SAVEDATA File"
        open.Filter = "(*.*)|*.*"
        open.FileName = "SAVEDATA"
        If (open.ShowDialog = Windows.Forms.DialogResult.OK) Then
            ProgressBar2.Visible = False
            Return open.FileName

            'Error catch
        Else
            ToolStripStatusLabel1.Text = "No File Selected!"
            MsgBox("Invalid File!", MsgBoxStyle.Exclamation, "")
            SaveToolStripMenuItem.Enabled = False
            Tabs.Visible = False
            ProgressBar2.Visible = False
        End If
        Return Nothing
    End Function

    'Runs OFD if "Open" is clicked from File menu
    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripMenuItem.Click
        ToolStripStatusLabel1.Text = "Opening File..."
        FilePath = OpenFile()
        If Not FilePath = Nothing Then
            Try
                ReadFile(FilePath)

                'Error catch
            Catch ex As Exception
                ProgressBar1.Visible = True
                ProgressBar1.Value = 0
                ToolStripStatusLabel1.Text = "Error Reading File!"
                MsgBox("Error Reading File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
            End Try
        End If
        ToolStripStatusLabel1.Text = "PacMan"
    End Sub

    'Saves file and calculates check
    Private Sub SaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripMenuItem.Click
        ToolStripStatusLabel1.Text = "Saving File..."
        Try
            WriteFile(FilePath)
            Checksum()
            ProgressBar1.Visible = True
            ProgressBar1.Value = 1
            MsgBox(("File Saved, checksum fixed!"))
            ProgressBar1.Visible = False
            ReadFile(FilePath)

            'Error catch
        Catch ex As Exception
            ToolStripStatusLabel1.Text = "Error Writing File!"
            MsgBox("Error Writing File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
        End Try
        ToolStripStatusLabel1.Text = "PacMan"
    End Sub
    'Reads the file selected as hex (little endian)
    Public Sub ReadFile(ByVal FilePath As String)
        Dim Reader As New PackageIO.Reader(FilePath, PackageIO.Endian.Little)

        'Start read position
        Reader.Position = &H0
        ToolStripStatusLabel2.Text = Reader.ReadInt32()

        'Correct file verification
        If ToolStripStatusLabel2.Text = "67121730" Then
            Try
                SaveToolStripMenuItem.Enabled = True
                Tabs.Visible = True

                'Reads addresses of all values containg level scores
                Reader.Position = &HD8
                lvl1score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HDC
                lvl2score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HE0
                lvl3score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HE4
                lvl4score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HE8
                lvl5score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HEC
                lvl6score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HF0
                lvl7score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HF4
                lvl8score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HF8
                lvl9score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &HFC
                lvl10score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &H100
                lvl11score.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &H15E
                totupg.Value = Reader.ReadInt32(Endian.Big)

                Reader.Position = &H1CC
                currlvlscore.Value = Reader.ReadInt32(Endian.Big)

                'Error catch
            Catch ex As Exception

                MsgBox("Error Reading File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
            End Try
        End If

        'File verification
        If ToolStripStatusLabel2.Text = "67121730" Then
            Try

                'Calculates actual value of lives enabled
                Reader.Position = &H164
                livesmath.Value = Reader.ReadUInt8()
                If (livesmath.Value <> 46 And livesmath.Value < 91) Then
                    CheckBox1.Checked = False
                    livesupg.Enabled = True
                    livesupg.Maximum = 5
                    livesdivi.Value = (livesmath.Value / 18)
                    livesupg.Value = livesdivi.Value
                ElseIf (livesmath.Value = 46) Then
                    CheckBox1.Enabled = False
                    CheckBox1.Checked = True
                    livesupg.Enabled = False
                    livesupg.Maximum = 51
                    livesupg.Value = 51
                End If

                'Enables/disables checkboxes if ammo is maxed or not
                Reader.Position = &H164
                ammo.Value = Reader.ReadInt16()
                If (ammo.Value = 1370) Then
                    CheckBox2.Checked = True
                    livesupg.Enabled = False
                End If

                'Calculates actual value of blob size
                Reader.Position = &H165
                sizemath.Value = Reader.ReadUInt16(Endian.Big)
                sizemath1.Value = Reader.ReadUInt8()
                If sizemath.Value = 18688 Then

                    sizedivi.Value = ((sizemath.Value - 18615) / divide.Value)
                    sizeupg.Value = sizedivi.Value

                ElseIf sizemath.Value = 37376 Then

                    sizedivi.Value = ((sizemath.Value - 37230) / divide.Value)
                    sizeupg.Value = sizedivi.Value

                ElseIf sizemath.Value = 56064 Then

                    sizedivi.Value = ((sizemath.Value - 55845) / divide.Value)
                    sizeupg.Value = sizedivi.Value

                ElseIf sizemath.Value = 1024 Then
                    sizeupg.Value = 4
                ElseIf sizemath.Value = 1280 Then
                    sizeupg.Value = 5
                End If
                Reader.Close(True, True)

                'Error catch
            Catch ex As Exception

                MsgBox("Error Reading File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
            End Try
        End If
    End Sub

    'Writes standard modified values to offsets
    Public Sub WriteFile(ByVal FilePath As String)
        Dim Writer As New PackageIO.IO(FilePath, PackageIO.Endian.Little)
        Dim Reader2 As New PackageIO.Reader(FilePath, PackageIO.Endian.Little)
        If ToolStripStatusLabel2.Text = "67121730" Then
            Try
                Writer.Position = &H8
                Writer.WriteInt32(0)

                Writer.Position = &HD8
                Writer.WriteInt32(lvl1score.Text, Endian.Big)

                Writer.Position = &HDC
                Writer.WriteInt32(lvl2score.Text, Endian.Big)

                Writer.Position = &HE0
                Writer.WriteInt32(lvl3score.Text, Endian.Big)

                Writer.Position = &HE4
                Writer.WriteInt32(lvl4score.Text, Endian.Big)

                Writer.Position = &HE8
                Writer.WriteInt32(lvl5score.Text, Endian.Big)

                Writer.Position = &HEC
                Writer.WriteInt32(lvl6score.Text, Endian.Big)

                Writer.Position = &HF0
                Writer.WriteInt32(lvl7score.Text, Endian.Big)

                Writer.Position = &HF4
                Writer.WriteInt32(lvl8score.Text, Endian.Big)

                Writer.Position = &HF8
                Writer.WriteInt32(lvl9score.Text, Endian.Big)

                Writer.Position = &HFC
                Writer.WriteInt32(lvl10score.Text, Endian.Big)

                Writer.Position = &H100
                Writer.WriteInt32(lvl11score.Text, Endian.Big)

                Writer.Position = &H15E
                Writer.WriteInt32(totupg.Value, Endian.Big)

                Writer.Position = &H162
                Writer.WriteUInt16(totupg.Value, Endian.Big)

                Writer.Position = &H1CC
                Writer.WriteInt32(currlvlscore.Text, Endian.Big)

                'Error catch
            Catch ex As Exception
                MsgBox("Could not save File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
            End Try
        End If

        'Writes calculated modified values to game calculation requirements
        Dim Writer2 As New PackageIO.IO(FilePath, PackageIO.Endian.Little)

        Reader2.Position = &H0
        ToolStripStatusLabel2.Text = Reader2.ReadInt32()

        If ToolStripStatusLabel2.Text = "67121730" Then
            Try

                Writer2.Position = &H164
                If (livesmath.Value < 91 And CheckBox1.Checked = False And CheckBox2.Checked = False) Then
                    lives.Value = (livesupg.Value * 18)
                    Writer2.WriteInt8(lives.Text)
                ElseIf (livesmath.Value < 91 And CheckBox1.Checked = True And CheckBox2.Checked = False) Then
                    lives.Value = ("150")
                    Writer2.WriteInt8(46)
                ElseIf (CheckBox2.Checked = True) Then
                    Writer2.WriteInt16(1370)
                End If

                Writer2.Position = &H165
                If sizeupg.Value = 1 Then

                    size.Value = 18688
                    Writer2.WriteUInt16(size.Text, Endian.Big)

                ElseIf sizeupg.Value = 2 Then

                    size.Value = 37376
                    Writer2.WriteUInt16(size.Text, Endian.Big)

                ElseIf sizeupg.Value = 3 Then

                    size.Value = 56064
                    Writer2.WriteUInt16(size.Text, Endian.Big)

                ElseIf sizeupg.Value = 4 Then
                    Writer2.WriteUInt8(sizeupg.Text)
                ElseIf sizeupg.Value = 5 Then
                    Writer2.WriteUInt8(sizeupg.Text)
                End If

                Writer2.Close(True, True)
                Reader2.Close(True, True)

                'Error catch
            Catch ex As Exception
                MsgBox("Could not save File!" & vbLf & ex.Message, MsgBoxStyle.Critical, "Error")
            End Try
        End If
    End Sub

    'Writes checksum to original check's location (nulled)
    Function Checksum()

        Dim IO As New PackageIO.IO(FilePath, Endian.Little)
        IO.Position = 8
        IO.WriteInt32(0)
        IO.Position = 4
        Dim len As Long = IO.ReadInt32(Endian.Big)
        IO.Position = 0
        Dim Buffer As Byte() = IO.ReadBytes(len)
        IO.Position = 8
        Dim crc As New CRC_DeBlob
        Dim hash As Array = crc.Compute(Buffer)
        Array.Reverse(hash)
        IO.Write(hash)
        IO.Close()

        Return Nothing
    End Function

    'General About info
    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Dim appInfo As New StringBuilder
        appInfo.AppendLine(ProductName)
        appInfo.AppendLine()
        appInfo.AppendLine("Version: " & ProductVersion)
        appInfo.AppendLine("Created by: PacMan for " & CompanyName)
        MsgBox(appInfo.ToString, MsgBoxStyle.Information, "About")

    End Sub

    'CRC 32 lzma (Slice-by-8) algo calculation table
    Public Class CRC_DeBlob

        Private Shared ReadOnly CRCTable As UInt32() = {&H0, &H77073096, &HEE0E612CUI, &H990951BAUI, &H76DC419, &H706AF48F, _
 &HE963A535UI, &H9E6495A3UI, &HEDB8832, &H79DCB8A4, &HE0D5E91EUI, &H97D2D988UI, _
 &H9B64C2B, &H7EB17CBD, &HE7B82D07UI, &H90BF1D91UI, &H1DB71064, &H6AB020F2, _
 &HF3B97148UI, &H84BE41DEUI, &H1ADAD47D, &H6DDDE4EB, &HF4D4B551UI, &H83D385C7UI, _
 &H136C9856, &H646BA8C0, &HFD62F97AUI, &H8A65C9ECUI, &H14015C4F, &H63066CD9, _
 &HFA0F3D63UI, &H8D080DF5UI, &H3B6E20C8, &H4C69105E, &HD56041E4UI, &HA2677172UI, _
 &H3C03E4D1, &H4B04D447, &HD20D85FDUI, &HA50AB56BUI, &H35B5A8FA, &H42B2986C, _
 &HDBBBC9D6UI, &HACBCF940UI, &H32D86CE3, &H45DF5C75, &HDCD60DCFUI, &HABD13D59UI, _
 &H26D930AC, &H51DE003A, &HC8D75180UI, &HBFD06116UI, &H21B4F4B5, &H56B3C423, _
 &HCFBA9599UI, &HB8BDA50FUI, &H2802B89E, &H5F058808, &HC60CD9B2UI, &HB10BE924UI, _
 &H2F6F7C87, &H58684C11, &HC1611DABUI, &HB6662D3DUI, &H76DC4190, &H1DB7106, _
 &H98D220BCUI, &HEFD5102AUI, &H71B18589, &H6B6B51F, &H9FBFE4A5UI, &HE8B8D433UI, _
 &H7807C9A2, &HF00F934, &H9609A88EUI, &HE10E9818UI, &H7F6A0DBB, &H86D3D2D, _
 &H91646C97UI, &HE6635C01UI, &H6B6B51F4, &H1C6C6162, &H856530D8UI, &HF262004EUI, _
 &H6C0695ED, &H1B01A57B, &H8208F4C1UI, &HF50FC457UI, &H65B0D9C6, &H12B7E950, _
 &H8BBEB8EAUI, &HFCB9887CUI, &H62DD1DDF, &H15DA2D49, &H8CD37CF3UI, &HFBD44C65UI, _
 &H4DB26158, &H3AB551CE, &HA3BC0074UI, &HD4BB30E2UI, &H4ADFA541, &H3DD895D7, _
 &HA4D1C46DUI, &HD3D6F4FBUI, &H4369E96A, &H346ED9FC, &HAD678846UI, &HDA60B8D0UI, _
 &H44042D73, &H33031DE5, &HAA0A4C5FUI, &HDD0D7CC9UI, &H5005713C, &H270241AA, _
 &HBE0B1010UI, &HC90C2086UI, &H5768B525, &H206F85B3, &HB966D409UI, &HCE61E49FUI, _
 &H5EDEF90E, &H29D9C998, &HB0D09822UI, &HC7D7A8B4UI, &H59B33D17, &H2EB40D81, _
 &HB7BD5C3BUI, &HC0BA6CADUI, &HEDB88320UI, &H9ABFB3B6UI, &H3B6E20C, &H74B1D29A, _
 &HEAD54739UI, &H9DD277AFUI, &H4DB2615, &H73DC1683, &HE3630B12UI, &H94643B84UI, _
 &HD6D6A3E, &H7A6A5AA8, &HE40ECF0BUI, &H9309FF9DUI, &HA00AE27, &H7D079EB1, _
 &HF00F9344UI, &H8708A3D2UI, &H1E01F268, &H6906C2FE, &HF762575DUI, &H806567CBUI, _
 &H196C3671, &H6E6B06E7, &HFED41B76UI, &H89D32BE0UI, &H10DA7A5A, &H67DD4ACC, _
 &HF9B9DF6FUI, &H8EBEEFF9UI, &H17B7BE43, &H60B08ED5, &HD6D6A3E8UI, &HA1D1937EUI, _
 &H38D8C2C4, &H4FDFF252, &HD1BB67F1UI, &HA6BC5767UI, &H3FB506DD, &H48B2364B, _
 &HD80D2BDAUI, &HAF0A1B4CUI, &H36034AF6, &H41047A60, &HDF60EFC3UI, &HA867DF55UI, _
 &H316E8EEF, &H4669BE79, &HCB61B38CUI, &HBC66831AUI, &H256FD2A0, &H5268E236, _
 &HCC0C7795UI, &HBB0B4703UI, &H220216B9, &H5505262F, &HC5BA3BBEUI, &HB2BD0B28UI, _
 &H2BB45A92, &H5CB36A04, &HC2D7FFA7UI, &HB5D0CF31UI, &H2CD99E8B, &H5BDEAE1D, _
 &H9B64C2B0UI, &HEC63F226UI, &H756AA39C, &H26D930A, &H9C0906A9UI, &HEB0E363FUI, _
 &H72076785, &H5005713, &H95BF4A82UI, &HE2B87A14UI, &H7BB12BAE, &HCB61B38, _
 &H92D28E9BUI, &HE5D5BE0DUI, &H7CDCEFB7, &HBDBDF21, &H86D3D2D4UI, &HF1D4E242UI, _
 &H68DDB3F8, &H1FDA836E, &H81BE16CDUI, &HF6B9265BUI, &H6FB077E1, &H18B74777, _
 &H88085AE6UI, &HFF0F6A70UI, &H66063BCA, &H11010B5C, &H8F659EFFUI, &HF862AE69UI, _
 &H616BFFD3, &H166CCF45, &HA00AE278UI, &HD70DD2EEUI, &H4E048354, &H3903B3C2, _
 &HA7672661UI, &HD06016F7UI, &H4969474D, &H3E6E77DB, &HAED16A4AUI, &HD9D65ADCUI, _
 &H40DF0B66, &H37D83BF0, &HA9BCAE53UI, &HDEBB9EC5UI, &H47B2CF7F, &H30B5FFE9, _
 &HBDBDF21CUI, &HCABAC28AUI, &H53B39330, &H24B4A3A6, &HBAD03605UI, &HCDD70693UI, _
 &H54DE5729, &H23D967BF, &HB3667A2EUI, &HC4614AB8UI, &H5D681B02, &H2A6F2B94, _
 &HB40BBE37UI, &HC30C8EA1UI, &H5A05DF1B, &H2D02EF8D}

        'Calculates checksum based on the table above
        Public Function Compute(ByVal s As Byte()) As Byte()
            Dim result As UInteger = &H0
            For Each b As Byte In s
                Dim sum1 As UInteger
                Dim sum2 As UInteger
                sum2 = (&H3FFFFFL And CUInt(b))
                sum1 = (result Xor sum2)
                result = (result >> 8) Xor CRC_DeBlob.CRCTable(sum1 And &HFF)
            Next
            Return BitConverter.GetBytes(Not result)
        End Function
    End Class

    'General How To info
    Private Sub HowToUseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HowToUseToolStripMenuItem.Click
        Dim appInfo As New StringBuilder
        appInfo.AppendLine("How to use:")
        appInfo.AppendLine()
        appInfo.AppendLine("Drage your save into Horizon's main screen")
        appInfo.AppendLine("Click Content")
        appInfo.AppendLine("Extract SAVEDATA")
        appInfo.AppendLine("Open SAVEDATA in this editor")
        appInfo.AppendLine("Mod what you want")
        appInfo.AppendLine("Save in editor (fixes checksum)")
        appInfo.AppendLine("Rehash & Resign with Horizon")

        MsgBox(appInfo.ToString, MsgBoxStyle.Information, "How To")
    End Sub

    'Buttons used for modifying values
    Private Sub PacToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PacToolStripMenuItem.Click
        Process.Start("http://www.360haven.com/forums/member.php?13255-PacMan")
    End Sub

    Private Sub HavenToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HavenToolStripMenuItem1.Click
        Process.Start("http://www.360haven.com")
    End Sub

    Private Sub lvl1max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl1max.Click
        lvl1score.Value = "2000000000"
    End Sub

    Private Sub lvl2max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl2max.Click
        lvl2score.Value = "2000000000"
    End Sub

    Private Sub lvl3max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl3max.Click
        lvl3score.Value = "2000000000"
    End Sub

    Private Sub lvl4max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl4max.Click
        lvl4score.Value = "2000000000"
    End Sub

    Private Sub lvl5max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl5max.Click
        lvl5score.Value = "2000000000"
    End Sub

    Private Sub lvl6max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl6max.Click
        lvl6score.Value = "2000000000"
    End Sub

    Private Sub lvl7max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl7max.Click
        lvl7score.Value = "2000000000"
    End Sub

    Private Sub lvl8max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl8max.Click
        lvl8score.Value = "2000000000"
    End Sub

    Private Sub lvl9max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl9max.Click
        lvl9score.Value = "2000000000"
    End Sub

    Private Sub lvl10max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl10max.Click
        lvl10score.Value = "2000000000"
    End Sub

    Private Sub lvl11max_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvl11max.Click
        lvl11score.Value = "2000000000"
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        totupg.Value = "30000"
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        livesupg.Enabled = True
        CheckBox1.Enabled = True
        CheckBox1.Checked = False
        livesupg.Value = "5"
        CheckBox2.Checked = False
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        sizeupg.Value = "5"
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        currlvlscore.Value = "2000000000"
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        lvl1score.Value = "2000000000"
        lvl2score.Value = "2000000000"
        lvl3score.Value = "2000000000"
        lvl4score.Value = "2000000000"
        lvl5score.Value = "2000000000"
        lvl6score.Value = "2000000000"
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        lvl7score.Value = "2000000000"
        lvl8score.Value = "2000000000"
        lvl9score.Value = "2000000000"
        lvl10score.Value = "2000000000"
        lvl11score.Value = "2000000000"
    End Sub

    Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        If (CheckBox2.Checked = True) Then
            livesupg.Enabled = False
            CheckBox1.Enabled = False
        ElseIf (CheckBox1.Checked = False) Then
            livesupg.Enabled = True
            CheckBox1.Enabled = True
        End If
    End Sub
End Class





