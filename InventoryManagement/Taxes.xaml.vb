Imports System.Data.SQLite
Imports System.Data
Public Class Taxes

    Dim applybydef As String
    Dim oramm As String


    Private Sub window_loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        loaddtgrid()
        loadID()
        applybydef = 0
        oramm = 0
    End Sub
    Private Sub button3_click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
    Sub loadID()
        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            Using cmd As New SQLiteCommand("SELECT IFNULL(MAX(TAXID), 0) FROM TAXES", con)
                con.Open()
                Using ad As New SQLiteDataAdapter(cmd)
                    Dim dt As New DataTable
                    ad.Fill(dt)

                    If dt.Rows.Count > 0 Then
                        ID.Text = Convert.ToInt32(dt.Rows(0)(0).ToString) + 1
                    Else
                        ID.Text = 1
                    End If
                End Using

            End Using
        End Using


    End Sub
    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If e.Key = Key.Escape Then

            Me.Close()

        End If
    End Sub

    Sub loaddtgrid()
        dtgrid.ItemsSource = Nothing
        Try


            Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
                Dim sqlstring As String = "Select TAXID As ID, TAXNAME As Name, TAX, PERCORAMOUNT, CASE WHEN PERCORAMOUNT = 1 THEN 'AMOUNT' WHEN PERCORAMOUNT = 0 THEN 'PERCENTAGE' ELSE 'ERROR CALL SUPPORT'     END AS Type
 FROM TAXES"
                con.Open()

                Using ad As New SQLiteDataAdapter(sqlstring, con)
                    Using dt As New DataTable
                        ad.Fill(dt)
                        dtgrid.ItemsSource = dt.DefaultView
                        dtgrid.Columns(3).Visibility = Visibility.Collapsed
                    End Using
                End Using
                con.Close()

            End Using
        Catch ex As SQLiteException
            MsgBox("SQL Error:  " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
            MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try

    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        If namelbl.Text = Nothing Then
            namelbl.Focus()
        ElseIf taxlbl.Text = Nothing Then
            taxlbl.Focus()
        ElseIf Percentage.IsChecked = False And Amount.IsChecked = False Then
            MsgBox("Select tax type.")

        Else
            Try
                Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))

                    Using cmd As New SQLiteCommand("INSERT INTO TAXES (TAXNAME, TAX, APPLYBYDEFAULT, PERCORAMOUNT) VALUES (@TNAME,@TAXAM,@APPLY,@PERCORAM)", con)


                        cmd.Parameters.AddWithValue("@TNAME", namelbl.Text)
                        cmd.Parameters.AddWithValue("@TAXAM", taxlbl.Text)
                        cmd.Parameters.AddWithValue("@APPLY", applybydef)
                        cmd.Parameters.AddWithValue("@PERCORAM", oramm)
                        con.Open()
                        cmd.ExecuteNonQuery()
                        MsgBox("New tax created successfully.")
                        loaddtgrid()
                        loadID()
                        namelbl.Text = Nothing
                        taxlbl.Text = Nothing
                        applydefaultcheck.IsChecked = False
                        Percentage.IsChecked = True
                        Amount.IsChecked = False

                    End Using

                End Using


            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
        End If

    End Sub

    Private Sub Percentage_Checked(sender As Object, e As RoutedEventArgs) Handles Percentage.Checked
        If Percentage.IsChecked = True Then
            Amount.IsChecked = False
        Else
            Amount.IsChecked = True

        End If
        Try

            If Percentage.IsChecked = True And Amount.IsChecked = False Then
            oramm = 0
        ElseIf Amount.IsChecked = True And Percentage.IsChecked = False Then
                oramm = 1


            End If
        Catch ex As SQLiteException
        MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
        MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub

    Private Sub Amount_Checked(sender As Object, e As RoutedEventArgs) Handles Amount.Checked
        If Amount.IsChecked = True Then
            Percentage.IsChecked = False
        Else
            Percentage.IsChecked = True
        End If
        Try


            If Percentage.IsChecked = True And Amount.IsChecked = False Then
            oramm = 0
        ElseIf Amount.IsChecked = True And Percentage.IsChecked = False Then
                oramm = 1


            End If
        Catch ex As SQLiteException
        MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

        Catch ex As Exception
        MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

        End Try
    End Sub

    Private Sub applydefaultcheck_Checked(sender As Object, e As RoutedEventArgs) Handles applydefaultcheck.Checked
        If applydefaultcheck.IsChecked = True Then
            applybydef = 1
        Else
            applybydef = 0
        End If
    End Sub
    Private Sub applydefaultcheck_unChecked(sender As Object, e As RoutedEventArgs) Handles applydefaultcheck.Unchecked
        If applydefaultcheck.IsChecked = False Then
            applybydef = 0
        Else
            applybydef = 1
        End If
    End Sub

    Private Sub dtgriwd_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtgrid.SelectionChanged
        Dim rowView = TryCast(dtgrid.SelectedItem, DataRowView)
        If rowView IsNot Nothing Then
            Dim firstColumnValue As Object = rowView(0)
            Try
                Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
                    Using cmd As New SQLiteCommand("SELECT * FROM TAXES where TAXID = @SEARCHID", con)

                        cmd.Parameters.AddWithValue("@SEARCHID", firstColumnValue)

                        con.Open()
                        Using reader As SQLiteDataReader = cmd.ExecuteReader
                            With reader.Read

                                ID.Text = reader("TAXID").ToString
                                taxlbl.Text = Convert.ToDecimal(reader("TAX"))
                                namelbl.Text = reader("TAXNAME")
                                If reader("APPLYBYDEFAULT") = 1 Then
                                    applydefaultcheck.IsChecked = True
                                ElseIf reader("APPLYBYDEFAULT") = 0 Then
                                    applydefaultcheck.IsChecked = False

                                End If
                                If reader("PERCORAMOUNT") = 1 Then
                                    Amount.IsChecked = True
                                    Percentage.IsChecked = False
                                Else
                                    Amount.IsChecked = False
                                    Percentage.IsChecked = True
                                End If
                            End With

                        End Using
                    End Using
                End Using

            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
        End If

        addtaxbtn.Visibility = Visibility.Collapsed
        updatetax.Visibility = Visibility.Visible

    End Sub
    Private Sub button4_click(sender As Object, e As RoutedEventArgs)
        If namelbl.Text = Nothing Then
            namelbl.Focus()
        ElseIf taxlbl.Text = Nothing Then
            taxlbl.Focus()
        ElseIf Percentage.IsChecked = False And Amount.IsChecked = False Then
            MsgBox("Select tax type.")

        Else
            Try
                Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))

                    Using cmd As New SQLiteCommand("UPDATE TAXES SET TAXNAME = @TNAME, TAX = @TAXAM, APPLYBYDEFAULT = @APPLY, PERCORAMOUNT = @PERCORAM WHERE TAXID = @TXID", con)


                        cmd.Parameters.AddWithValue("@TNAME", namelbl.Text)
                        cmd.Parameters.AddWithValue("@TAXAM", taxlbl.Text)
                        cmd.Parameters.AddWithValue("@APPLY", applybydef)
                        cmd.Parameters.AddWithValue("@PERCORAM", oramm)
                        cmd.Parameters.AddWithValue("@TXID", ID.Text)
                        con.Open()
                        cmd.ExecuteNonQuery()
                        MsgBox("Tax " & namelbl.Text & " updated successfully.")
                        loaddtgrid()

                    End Using

                End Using


            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
        End If

    End Sub

    Private Sub New_Click_1(sender As Object, e As RoutedEventArgs) Handles [New].Click
        addtaxbtn.Visibility = Visibility.Visible
        updatetax.Visibility = Visibility.Collapsed
        loaddtgrid()
        loadID()
        namelbl.Text = Nothing
        taxlbl.Text = Nothing
        applydefaultcheck.IsChecked = False
        Percentage.IsChecked = True
        Amount.IsChecked = False


    End Sub
End Class
