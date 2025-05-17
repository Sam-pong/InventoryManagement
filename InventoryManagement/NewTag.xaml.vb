Imports System.Data.SQLite
Public Class NewTag

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
    Sub loadID()
        Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
            TagName.Text = Nothing
            Try
                con.Open()
                Using com As New SQLiteCommand("SELECT max(TagID) FROM TAGS", con)
                    Using RDR = com.ExecuteReader
                        If RDR.HasRows Then
                            Do While RDR.Read
                                Dim TagIDint As Integer
                                TagIDint = RDR.Item("max(TagID)")

                                TagID.Text = TagIDint + 1

                            Loop
                        Else
                            TagID.Text = "1"
                        End If

                    End Using

                End Using

            Catch ex As SQLiteException
                MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

            Catch ex As Exception
                MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

            End Try
            con.Close()

        End Using
    End Sub
    Private Sub NewTag_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        loadID()
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        If TagName.Text = Nothing Then
            TagName.Focus()
        Else
            Using con As New SQLiteConnection(CStr("Data source = InvenManage.db"))
                Using com As New SQLiteCommand("INSERT INTO TAGS (TagName) VALUES (@TagNameParam)", con)
                    com.Parameters.AddWithValue("@TagNameParam", TagName.Text)
                    con.Open()
                    Try
                        com.ExecuteNonQuery()
                        loadID()
                    Catch ex As SQLiteException
                        MsgBox("SQL Error: " & ex.Message, MsgBoxStyle.Critical, "Database Error")

                    Catch ex As Exception
                        MsgBox("Unexpected Error: " & ex.Message, MsgBoxStyle.Critical, "Error")

                    End Try
                    con.Close()
                End Using
            End Using

        End If
    End Sub
End Class
