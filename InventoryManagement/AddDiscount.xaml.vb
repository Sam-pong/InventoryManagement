Public Class AddDiscount
    Public Property DiscountAdded As Decimal
    Public Property itemortotal As Integer




    Private Sub CheckBox_Checked(sender As Object, e As RoutedEventArgs)
        If peritemcheck.IsChecked = True Then

            peritemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00FF27"))
            totalitemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))
            totalitemcheck.IsChecked = False
        Else
            totalitemcheck.IsChecked = True
            totalitemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00FF27"))
            peritemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))


        End If
    End Sub

    Private Sub totalitemcheck_Checked(sender As Object, e As RoutedEventArgs) Handles totalitemcheck.Checked
        If totalitemcheck.IsChecked = True Then
            peritemcheck.IsChecked = False
            totalitemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00FF27"))
            peritemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))

        Else
            peritemcheck.IsChecked = True
            peritemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00FF27"))
            totalitemcheck.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))

        End If
    End Sub

    Private Sub window_loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        peritemcheck.IsChecked = True
        totalitemcheck.IsChecked = False
        bakcend.Text = DiscountAdded
        DiscountAdjust.Text = DiscountAdded
        DiscountAdjust.Focus()
        DiscountAdjust.CaretIndex = DiscountAdjust.Text.Length
        itemortotal = 0
    End Sub



    Private Sub Button3_Click(sender As Object, e As RoutedEventArgs)
        DiscountAdded = bakcend.Text
        Me.Close()

    End Sub




    Private Sub Enterbutton_Click(sender As Object, e As RoutedEventArgs) Handles Enterbutton.Click

        If Decimal.TryParse(DiscountAdjust.Text, DiscountAdded) Then
            If peritemcheck.IsChecked = True Then
                itemortotal = 0
                DialogResult = True
                Me.Close()
            Else
                itemortotal = 1
                DialogResult = True
                Me.Close()


            End If
        Else
            MessageBox.Show("Please enter a valid number.")
        End If
    End Sub

    Sub sendthing()
        If Decimal.TryParse(DiscountAdjust.Text, DiscountAdded) Then
            If peritemcheck.IsChecked = True Then
                itemortotal = 0
                DialogResult = True
                Me.Close()
            Else
                itemortotal = 1
                DialogResult = True
                Me.Close()


            End If
        Else
            MessageBox.Show("Please enter a valid number.")
        End If
    End Sub
    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If e.Key = Key.Escape Then
            DiscountAdded = bakcend.Text
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
