

$(document).ready(function () {
        


    $(function () {
        $("#tabs").tabs();

        
    });

    var opts = {
        lines: 13, // The number of lines to draw
        length: 7, // The length of each line
        width: 4, // The line thickness
        radius: 10, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        color: '#000', // #rgb or #rrggbb
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto' // Left position relative to parent in px
    };

    $(".search-item a").click(function () {
        var id = $(this).closest(".search-item").attr("id");
        $("#" + id).css("opacity", "0.7");
        id = document.getElementById(id)
        var spinner = new Spinner(opts).spin(id);
    });


    $("#btnUpdate").click(function () {
        $(this).hide();
        $(".loading").css("display", "block");
    });

    //$("#addLocation").click(function () {
    //    //alert("test");
    //    $("#validateForm").val("addLocation");
    //    $("#formElem").submit();
    //});

    $(".favourite span a").click(function () {
        $(".search-item").css("opacity", "1");
        $(".spinner").remove();

        var id = $(this).data("id");
        var task = $(this).data("task");

        $(this).text("updating...");

        $.get($(location).attr('origin') + '/json/Function/AddFavourite?task=' + task + 'favourite&id=' + id, function (data) {
            if (data.trim() == "Added") {
                $(".fav_" + id + " a").text("Added to your favourites");
                $(".fav_" + id + " a").data("task", "remove");
            } else if (data.trim() == "Removed") {
                $(".fav_" + id + " a").text("Interested in going?");
                $(".fav_" + id + " a").data("task", "add");
            } else if (data.trim() == "login") {
                window.location.replace($(location).attr('origin') + '/Account/Login?returnUrl=' + $(location).attr('href'));
            }
        });
    });

    function addToFavourite(id, task)
    {
        $.get($(location).attr('host') + '/json/Function/AddFavourite?task=' + task + 'favourite&id=' + id, function (data) {
            if (data.trim() == "True") {
                $(this).text("Added");
                //$("#saving").hide();
                //$("#save").fadeIn(600).delay(2000).fadeOut(600);
            } else if (data.trim() == "login") {
                //$(this).text("failed to add");
                //$("#saving").hide();
                //$("#fail").fadeIn(600).delay(4000).fadeOut(600);
            }
        });

    };

    $("#claim-ownership").click(function () {
        var company = $(this).data("id");

        if ($(this).hasClass("sent")) {

        }
        else {
            $(this).addClass("sent");
            
            $(this).text("Sending your request...");

            $.get($(location).attr('origin') + '/json/Function/AddFavourite?task=claimOwnership&company=' + company, function (data) {
                if (data.trim() == "Request Sent") {
                    $("#claim-ownership").text("Request Sent. We'll be in touch shortly.");
                } else if (data.trim() == "login") {
                    window.location.replace($(location).attr('origin') + '/Account/Login?returnUrl=' + $(location).attr('href'));
                }
                else {
                    $("#claim-ownership").text("Unable to send request at this time");
                }
            });
        }
    });


    $(".class-archive-head").click(function () {
        if ($(".class-archive-list").css("display") == "none") {
            $(".class-archive-list").css("display", "block");
        }
        else {
            $(".class-archive-list").css("display", "none");
        }
    });
    


});