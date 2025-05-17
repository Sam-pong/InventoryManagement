Imports System.Data.SQLite
Imports System.Data
Public Class NewItemWindow
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Sub FillTAGCOMBO()
        TAGCOMBO.Items.Clear()

        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Using com As New SQLiteCommand("SELECT * FROM TAGS", con)
                con.Open()

                Try
                    Dim dt As New DataTable
                    Dim ad As New SQLiteDataAdapter(com)
                    ad.Fill(dt)
                    TAGCOMBO.ItemsSource = dt.DefaultView
                    TAGCOMBO.DisplayMemberPath = "TAGNAME"
                    TAGCOMBO.SelectedValue = "TAGID"
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try

            End Using
            con.Close()

        End Using
    End Sub

    Sub ItemIDFill()
        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Using com As New SQLiteCommand("SELECT Max(SerielNumber) FROM ITEMS", con)
                Try
                    con.Open()

                    Using RDR = com.ExecuteReader
                        If RDR.HasRows Then
                            Do While RDR.Read
                                Dim ItemIDINT As Integer
                                ItemIDINT = RDR.Item("Max(SerielNumber)")

                                ItemID.Text = ItemIDINT + 1

                            Loop
                        Else
                            ItemID.Text = 1
                        End If

                    End Using
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try
            End Using
            con.Close()


        End Using

    End Sub
    Private Sub NewItemWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        FillTAGCOMBO()
        ItemIDFill()
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim NewT As New NewTag
        Me.Close()
        NewT.ShowDialog()
    End Sub

    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
        If nametxt.Text = Nothing Then
            nametxt.Focus()
        ElseIf desctxt.Text = Nothing Then
            desctxt.Focus()
        ElseIf locationtxt.Text = Nothing Then
            locationtxt.Focus()
        ElseIf purchasetxt.Text = Nothing Then
            purchasetxt.Focus()
        ElseIf retailtxt.Text = Nothing Then
            retailtxt.Focus()
        ElseIf barcodetxt.Text = Nothing Then
            barcodetxt.Focus()
        End If
        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Try


                Using cmd As New SQLiteCommand("INSERT INTO ITEMS (ItemName, Desc, TagID, Location, Stock, PurchasePrice, RetailPrice, Barcode) VALUES 
(@NameItem, @Description, @IDTag, @Loc, @Stock, @Purchase, @Retail, @Barc)", con)
                    cmd.Parameters.AddWithValue("@NameItem", nametxt.Text)
                    cmd.Parameters.AddWithValue("@Description", desctxt.Text)
                    cmd.Parameters.AddWithValue("@IDTag", TAGCOMBO.SelectedValue)
                    cmd.Parameters.AddWithValue("@Loc", locationtxt.Text)
                    cmd.Parameters.AddWithValue("@Stock", stocktxt.Text)
                    cmd.Parameters.AddWithValue("@Purchace", purchasetxt.Text)
                    cmd.Parameters.AddWithValue("@Retail", retailtxt.Text)
                    cmd.Parameters.AddWithValue("@Barc", barcodetxt.Text)

                    con.Open()
                    cmd.ExecuteNonQuery()
                    con.Close()
                End Using
            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
        End Using

    End Sub
End Class
