$(document).ready(function () {
    // Callback để đảm bảo CAPTCHA đã sẵn sàng
    window.recaptchaReady = function () {
        console.log("reCAPTCHA is ready");
        $("#btnLogin").prop("disabled", false); // Enable nút Login
        $("#btnReg").prop("disabled", false);   // Enable nút Register
    };

    // Vô hiệu hóa nút Login và Register cho đến khi CAPTCHA sẵn sàng
    $("#btnLogin").prop("disabled", true);
    $("#btnReg").prop("disabled", true);

    // Retry logic nếu CAPTCHA không tải được
    function waitForRecaptcha(attempts, maxAttempts, callback) {
        if (typeof grecaptcha !== "undefined" && grecaptcha.getResponse) {
            callback();
        } else if (attempts < maxAttempts) {
            console.log(`Waiting for reCAPTCHA... Attempt ${attempts + 1}/${maxAttempts}`);
            setTimeout(function () {
                waitForRecaptcha(attempts + 1, maxAttempts, callback);
            }, 1000);
        } else {
            console.error("reCAPTCHA failed to load after maximum attempts");
            Lobibox.notify("error", {
                msg: "Không thể tải CAPTCHA, vui lòng làm mới trang"
            });
            $("#btnLogin").prop("disabled", true);
            $("#btnReg").prop("disabled", true);
        }
    }

    // Chờ CAPTCHA sẵn sàng
    waitForRecaptcha(0, 10, function () {
        console.log("reCAPTCHA loaded successfully");
        window.recaptchaReady();
    });

    // Xử lý form đăng nhập
    $("#frmLogin").on("submit", function (e) {
        e.preventDefault();

        var username = $("#Username").val();
        var password = $("#Password").val();
        var recaptchaResponse = grecaptcha.getResponse();

        console.log("reCAPTCHA response before submit (login):", recaptchaResponse);

        if (!username || !password) {
            Lobibox.notify("error", {
                msg: "Vui lòng nhập đầy đủ thông tin"
            });
            return;
        }

        if (!recaptchaResponse) {
            console.log("reCAPTCHA response is empty (login)");
            Lobibox.notify("error", {
                msg: "Vui lòng xác nhận bạn không phải là robot"
            });
            return;
        }

        $.ajax({
            url: "/Account/CheckLogin",
            type: "POST",
            data: {
                Username: username,
                Password: password,
                gRecaptchaResponse: recaptchaResponse
            },
            success: function (response) {
                if (response.status) {
                    Lobibox.notify("success", {
                        msg: response.mess
                    });
                    // Tự động chuyển hướng sau 1 giây
                    setTimeout(function () {
                        let rtUrl = $('#rtUrl').val();
                        if (!rtUrl) {
                            rtUrl = "/";
                        }
                        window.location.href = rtUrl;
                    }, 1000);
                } else {
                    Lobibox.notify("error", {
                        msg: response.mess
                    });
                    grecaptcha.reset();
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX error (login):", status, error);
                Lobibox.notify("error", {
                    msg: "Có lỗi xảy ra, vui lòng thử lại"
                });
                grecaptcha.reset();
            }
        });
    });

    // Xử lý form đăng ký
    $("#frmReg").on("submit", function (e) {
        e.preventDefault();

        var recaptchaResponse = grecaptcha.getResponse();

        console.log("reCAPTCHA response before submit (register):", recaptchaResponse);

        if (!recaptchaResponse) {
            console.log("reCAPTCHA response is empty (register)");
            Lobibox.notify("error", {
                msg: "Vui lòng xác nhận bạn không phải là robot"
            });
            return;
        }

        var formData = new FormData(this);
        formData.append("gRecaptchaResponse", recaptchaResponse);

        $.ajax({
            url: "/Account/Register",
            type: "POST",
            data: formData,
            success: function (response) {
                if (response.status) {
                    Lobibox.notify("success", {
                        msg: response.mess
                    });
                    // Tự động chuyển hướng sau 1 giây
                    setTimeout(function () {
                        let rtUrl = $('#rtUrl').val();
                        if (!rtUrl) {
                            rtUrl = "/";
                        }
                        window.location.href = rtUrl;
                    }, 1000);
                } else {
                    Lobibox.notify("error", {
                        msg: response.mess
                    });
                    grecaptcha.reset();
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX error (register):", status, error);
                Lobibox.notify("error", {
                    msg: "Có lỗi xảy ra, vui lòng thử lại"
                });
                grecaptcha.reset();
            },
            cache: false,
            contentType: false,
            processData: false
        });
    });

    // Xử lý sự kiện Enter trên input
    $('input').keypress(function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $("#frmLogin").submit();
            return false;
        }
    });

    // Xử lý nút Login
    $('#btnLogin').click(function (e) {
        e.preventDefault();
        $("#frmLogin").submit();
    });
});