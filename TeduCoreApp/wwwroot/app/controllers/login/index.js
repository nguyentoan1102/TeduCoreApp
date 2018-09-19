var loginController = function () {
    this.initialize = function () {
        registerEvents();
    };
    var registerEvents = function () {
        $('#frmLogin').validate({
            errorClass: 'red',
            ignore: [],
            lang: 'en',
            rules: {
                userName: {
                    required: true
                },
                password: {
                    required: true
                }
            },
            messages: {
                userName: ' userName required!',
                password: ' password required!'
            }
        });
        $('#btnLogin').on('click', function (e) {
            e.preventDefault();
            if ($('#frmLogin').valid()) {
                e.preventDefault();
                var user = $('#txtUserName').val();
                var password = $('#txtPassword').val();
                login(user, password);
            }
        });
    }
    var login = function (user, pass) {
        $.ajax({
            type: 'POST',
            data: {
                UserName: user,
                Password: pass
            },
            dataType: 'json',
            //url: '@Url.Action("login", "authen",new {Area="admin"})',
            url: "admin/login/authen",
            success: function (res) {
                if (res.Success) {
                    window.location.href = "/Admin/Home/Index";
                }
                else {
                    tedu.notify('Login failed', 'error');
                }
            },
            error: function () { console.log("Error") }
        });
    }
}