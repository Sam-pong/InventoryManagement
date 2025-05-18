Imports System.Data.SQLite
Imports System.Data
Public Class NewItemWindow
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Sub FillTAGCOMBO()
        TAGCOMBO.ItemsSource = Nothing
        TAGCOMBO.Items.Clear()
        nametxt.Text = Nothing
        desctxt.Text = Nothing
        locationtxt.Text = Nothing
        purchasetxt.Text = Nothing
        retailtxt.Text = Nothing
        barcodetxt.Text = Nothing
        stocktxt.Text = Nothing

        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Using com As New SQLiteCommand("SELECT * FROM TAGS", con)
                con.Open()

                Try
                    Dim dt As New DataTable
                    Dim ad As New SQLiteDataAdapter(com)
                    ad.Fill(dt)
                    TAGCOMBO.ItemsSource = dt.DefaultView
                    TAGCOMBO.DisplayMemberPath = "TAGNAME"
                    TAGCOMBO.SelectedValuePath = "TAGID"
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
            Using com As New SQLiteCommand("SELECT IFNULL(MAX(SerielNumber), 0) FROM ITEMS", con)
                Try
                    con.Open()

                    Using ad As New SQLiteDataAdapter(com)
                        Dim dt As New DataTable
                        ad.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            Dim maxID As Integer = Convert.ToInt32(dt.Rows(0)(0))
                            ItemID.Text = (maxID + 1).ToString()
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
        ElseIf TAGCOMBO.SelectedIndex < 0 Then
            TAGCOMBO.Focus()
        ElseIf locationtxt.Text = Nothing Then
            locationtxt.Focus()


        ElseIf stocktxt.Text = Nothing Then
            stocktxt.Focus()

        ElseIf purchasetxt.Text = Nothing Then
            purchasetxt.Focus()
        ElseIf retailtxt.Text = Nothing Then
            retailtxt.Focus()

        ElseIf barcodetxt.Text = Nothing Then
            barcodetxt.Focus()
        Else
            Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
                Try


                    Using cmd As New SQLiteCommand("INSERT INTO ITEMS (ItemName, Desc, TagID, Location, Stock, PurchasePrice, RetailPrice, Barcode) VALUES 
(@NameItem, @Description, @IDTag, @Loc, @Stock, @Purchase, @Retail, @Barc)", con)
                        cmd.Parameters.AddWithValue("@NameItem", nametxt.Text)
                        cmd.Parameters.AddWithValue("@Description", desctxt.Text)
                        cmd.Parameters.AddWithValue("@IDTag", TAGCOMBO.SelectedValue)
                        cmd.Parameters.AddWithValue("@Loc", locationtxt.Text)
                        cmd.Parameters.AddWithValue("@Stock", stocktxt.Text)
                        cmd.Parameters.AddWithValue("@Purchase", purchasetxt.Text)
                        cmd.Parameters.AddWithValue("@Retail", retailtxt.Text)
                        cmd.Parameters.AddWithValue("@Barc", barcodetxt.Text)

                        con.Open()
                        cmd.ExecuteNonQuery()
                        MsgBox("Success!")
                        FillTAGCOMBO()
                        ItemIDFill()

                        con.Close()
                    End Using
                Catch ex As SQLiteException
                    MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                Catch ex As Exception
                    MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                End Try
            End Using

        End If



    End Sub
End Class
