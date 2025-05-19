Public Class EditQuanitity
    Public Property UpdatedQuantity As Integer

    Private Sub Button3_Click(sender As Object, e As RoutedEventArgs)
        UpdatedQuantity = bakcend.Text
        Me.Close()

    End Sub


    Private Sub editquant_loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        bakcend.Text = UpdatedQuantity
        QuantityAdjust.Text = UpdatedQuantity
        QuantityAdjust.Focus()
        QuantityAdjust.CaretIndex = QuantityAdjust.Text.Length


    End Sub

    Private Sub Enterbutton_Click(sender As Object, e As RoutedEventArgs) Handles Enterbutton.Click

        If Integer.TryParse(QuantityAdjust.Text, UpdatedQuantity) Then
            DialogResult = True
            Me.Close()
        Else
            MessageBox.Show("Please enter a valid number.")
        End If
    End Sub

    Sub sendthing()
        If Integer.TryParse(QuantityAdjust.Text, UpdatedQuantity) Then
            DialogResult = True
            Me.Close()
        Else
            MessageBox.Show("Please enter a valid number.")
        End If
    End Sub
    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If e.Key = Key.Escape Then
            UpdatedQuantity = bakcend.Text
            Me.Close()
        End If


    End Sub
    Private Sub thing_keydown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Key = Key.Enter Then
            Enterbutton_Click(Enterbutton, New RoutedEventArgs())
            e.Handled = True
        End If

    End Sub
End Class
