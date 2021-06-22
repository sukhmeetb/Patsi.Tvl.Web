$(function () {
    Toggle_Password();
});


function Toggle_Password() {
    $('#togglePassword').click(function () {

        if ($(this).hasClass('fa-eye')) {

            $(this).removeClass('fa-eye');

            $(this).addClass('fa-eye-slash');

            $('#password').attr('type', 'text');

        } else {

            $(this).removeClass('fa-eye-slash');

            $(this).addClass('fa-eye');

            $('#password').attr('type', 'password');
        }
    });
}