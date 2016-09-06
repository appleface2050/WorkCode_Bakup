function initLocalization() {
    console.log('in index in initLocalization()');
    var langArray = ["ar-AE","de-DE","en-US","es-ES","fr-FR","id-ID","it-IT","ja-JP","ko-KR","pt-BR","ru-RU","th-TH","tl-PH","tr-TR","vi-VN","zh-CN","zh-TW"];
    $(".i18n").each(function () {
        $(this).html(en_US[$(this).attr("label")]);
    });
    var userLang;
    var header = document.createElement('script');


    //For Gm


    if (document.location.hostname == "localhost_temp") {
        // change here for localhost locale testing
        userLang = "en-US";
        if($.inArray(userLang,langArray) == -1)
            userLang = "en-US"
    }
    else {

        userLang = navigator.language || navigator.userLanguage;
        if($.inArray(userLang,langArray) == -1)
            userLang = "en-US"
    }


    console.log(userLang);

    header.src = './i18n/' + userLang + '.js';
    header.type = 'text/javascript';
    document.head.appendChild(header);
    var userLang1 = userLang.replace('-', '_');
    var gotTranslations = false;
    var get_data = setInterval(function () {
        if (!gotTranslations) {
            var get_lang_data = eval(userLang1);
            if (get_lang_data != null || get_lang_data != undefined) {
                localised_tutorial_strings = get_lang_data;
                console.log(localised_tutorial_strings);
                if(localised_tutorial_strings!=undefined)
                    clearInterval(get_data);
                function showpkg_name() {
                    $(".i18n").each(function () {
                        clearInterval(get_data);
                        $(this).html(eval(userLang1)[$(this).attr("label")]), $(this).html(eval(userLang1)[$(this).attr('label')]), $(this).attr("placeholder", eval(userLang1)[$(this).attr("label")]);
                    });
                }

                setInterval(showpkg_name, 500)
            }
            else {
                function showpkg_name() {
                    $(".i18n").each(function () {
                        clearInterval(get_data);
                        $(this).html(eval(en_US)[$(this).attr("label")]), $(this).html(eval(en_US)[$(this).attr('label')]), $(this).attr("placeholder", en_US[$(this).attr("label")]);

                    });
                }

                setInterval(showpkg_name, 500)
            }
        }
        else {
            gotTranslations = true;
        }
    }, 10);
}
