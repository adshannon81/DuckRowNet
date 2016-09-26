/// <reference path="C:\Users\AJS\Documents\Visual Studio 2015\Projects\DuckRowNet\DuckRowNet\eventList.cshtml" />

function log(s) {
    var log_container = document.getElementById('eventList');
    if (log_container) {
        var text = document.createTextNode(s);
        var br = document.createElement("br");
        log_container.appendChild(text);
        log_container.appendChild(br);
    }
}

(function () {

    // Localize jQuery variable
    var jQuery;

    /******** Load jQuery if not present *********/
    if (window.jQuery === undefined || window.jQuery.fn.jquery !== '1.7.2') {
        var script_tag = document.createElement('script');
        script_tag.setAttribute("type", "text/javascript");
        script_tag.setAttribute("src",
        "http://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js");
        if (script_tag.readyState) {
            script_tag.onreadystatechange = function () { // For old versions of IE
                if (this.readyState == 'complete' || this.readyState == 'loaded') {
                    scriptLoadHandler();
                }
            };
        } else { // Other browsers
            script_tag.onload = scriptLoadHandler;
        }
        // Try to find the head, otherwise default to the documentElement
        (document.getElementsByTagName("head")[0] || document.documentElement).appendChild(script_tag);
    } else {
        // The jQuery version on the window is the one we want to use
        jQuery = window.jQuery;
        main();
    }

    /******** Called once jQuery has loaded ******/
    function scriptLoadHandler() {
        // Restore $ and window.jQuery to their previous values and store the
        // new jQuery in our local jQuery variable
        jQuery = window.jQuery.noConflict(true);
        // Call our main function
        main();
    }

    /******** Our main function ********/
    function main() {
        jQuery(document).ready(function ($) {
            $(".eventList").each(function () {

                var elCompany = $(this).data("company"); //the company name
                var elView = $(this).data("view"); //the display style; full, block, other
                var elSubCategories = $(this).data("subcategories"); //only show for subcategories; seperate by comma
                var elInstructor = $(this).data("instructorid"); //only show for instructors; seperate by comma
                var elActive = $(this).data("active"); //show only future classes, currently running or all active; future-only, running-only, all; 
                var elHeader = $(this).data("header"); //option to show a custom header

                var strLink = "";

                var urlHost = "";


                if (typeof elCompany != "undefined") {
                    strLink = "http://duckrow.net/json/EventList?name=" + encodeURIComponent(elCompany);

                    if (typeof elSubCategories != "undefined") {
                        strLink = strLink + "&subcategories=" + encodeURIComponent(elSubCategories);
                    }
                    if (typeof elInstructor != "undefined") {
                        strLink = strLink + "&instructor=" + encodeURIComponent(elInstructor)
                    }
                    if (typeof elActive != "undefined") {
                        strLink = strLink + "&active=" + encodeURIComponent(elActive)
                    }

                    if (elView == "full") {
                        strLink = strLink + "&detail=full";

                        var eventbox = $(this);
                        eventbox.html('<div style="text-align:center;line-height: 80px;"><img src="http://duckrow.net/Images/ajax-loader-bar.gif" /></div>');

                        jQuery.ajax({
                            type: 'GET',
                            url: strLink,
                            contentType: "application/json; charset=utf-8",
                            dataType: 'jsonp',
                            success: function (data) {
                                var row = 0;
                                eventbox.empty();
                                var list = '';
                                $.each(data.events, function (i, event) {

                                    if (row == 0) {
                                        if (typeof elHeader != "undefined") {
                                            eventbox.append('<h2>' + elHeader + '</h2>');
                                        }
                                        list = '<table class="qt_eventListTable"> ' +
                                        '<tr class="qt_eventListTable_Head"> ' +
                                        '<td style="width:150px;">Starting On</td> ' +
                                        '<td style="width:100px;">Time</td> ' +
                                        '<td style="width:180px;">Name</td> ' +
                                        '<td style="width:120px;">Sub-Category</td> ' +
                                        '<td style="width:120px;">Location</td> ' +
                                        '<td style="width:80px;"></td> ' +
                                        '</tr>';
                                    }

                                    if (typeof event === 'undefined') {
                                        //this check is needed for IE to prevent errrors
                                        return false;
                                    }
                                    else {
                                        var full = '';
                                        if (event.remainingCapacity == 0) {
                                            full = ' qt_eventListTable_Row-Full ';
                                        }
                                        list = list + '<tr class="qt_eventListTable_Row ' + full + '"  onclick="parent.location = \'' + event.url + '\'" > ' +
                                            '<td>' + event.startDate + '</td> ' +
                                            '<td>' + event.startTime + '</td> ' +
                                            '<td>' + event.name + '</td> ' +
                                            '<td>' + event.subCategory + '</td> ' +
                                            '<td>' + event.location + '</td> ' +
                                            '<td>Book Now</td> ' +
                                            '</tr> ';
                                        row++;
                                    }
                                    //if (i == 10) return false;
                                });

                                if (row == 0) {
                                    list = list + '<a href="http://duckrow.net/' + elCompany + '">http://duckrow.net/' + elCompany + '</a>';
                                }
                                else {
                                    list = list + '</table>';
                                    eventbox.append('<link href="http://duckrow.net/Widgets/style.css")" rel="Stylesheet" />');
                                }
                                eventbox.append(list);
                            }
                        });

                    }
                    else if (elView == "block") {
                        strLink = strLink + "&detail=full";

                        var eventbox = $(this);
                        eventbox.html('<div style="text-align:center"><img src="http://duckrow.net/Images/ajax-loader-bar.gif" /></div>');

                        jQuery.ajax({
                            type: 'GET',
                            url: strLink,
                            dataType: 'jsonp',
                            success: function (data) {
                                var row = 0;
                                eventbox.empty();
                                if (typeof elHeader != "undefined") {
                                    eventbox.append('<h2>' + elHeader + '</h2>');
                                }
                                var list = '<div class="qt_eventListBlocks row" >';

                                $.each(data.events, function (i, event) {
                                    if (typeof event === 'undefined') {
                                        //this check is needed for IE to prevent errrors
                                        return false;
                                    }
                                    else {
                                        
                                        list = list + ''
                                            + '<div class="search-item '+event.cat + '" onclick="parent.location = \'' + event.url + '\'">'
                                            + '<div class="search-item-image">'
                                                + '<a href="' + event.url +'"><img src="' +event.image +'" style="" alt="' + event.name +'"></a>'
                                            + '</div>'
                                            + '<div class="search-item-detail">'
                                             + '<div class="event-wrapper">'
                                              + '<div class="event-title">'
                                               + '<span class="date">' + event.startDay + '<span>' + event.startMonth + '</span></span>'
                                               + '<h2 class="entry-title" itemprop="name"><a itemprop="url" href="' + event.url + '" title="' + event.name + '" rel="bookmark">' +event.name + '</a></h2>'
                                              + '</div>'
                                              + '<div class="entry-details">';

                                        var startText = '<p class="start">Starting On</p>';
                                        if (event.startDate < new Date()) {
                                            startText = '<p class="start">Next Date</p>';
                                        }

                                        list = list + startText 
                                               + '<p class="event_date"><span><span >' + event.nextDate + '</span> </span><span class="event-time">' + event.nextTime + '</span></p>'
                                               + '<p class="address">' + event.location + '</p>'
                                              + '</div>'
                                             + '</div>'
                                            + '</div>';
                                        
                                        if (event.remainingCapacity == 0) {
                                            list = list + '<div class="fully-booked">' +
                                                '<img src="http://duckrow.net/Images/fullybooked_small.png" alt=""> ' +
                                                '</div>';
                                        }

                                        list = list + "</div>";

                                        row++;

                                        if (row % 3 === 0) {
                                            list = list + '<div class="clear-fix"></div>';
                                        }

                                    }

                                });

                                if (row == 0) {
                                    list = list + '<a href="http://duckrow.net/' + elCompany + '">http://duckrow.net/' + elCompany + '</a>';
                                }

                                list = list + '</div>';
                                eventbox.append('<link href="http://duckrow.net/Widgets/style.css")" rel="Stylesheet" />');

                                eventbox.append(list);
                            }
                        });

                    }
                    else {
                        var eventbox = $(this);
                        eventbox.html('<div style="text-align:center"><img src="http://duckrow.net/Images/ajax-loader-bar.gif" /></div>');

                        jQuery.ajax({
                            type: 'GET',
                            url: strLink,
                            dataType: 'jsonp',
                            success: function (data) {
                                var row = 0;
                                eventbox.empty();
                                var list = '';
                                $.each(data.events, function (i, event) {
                                    if (row == 0) {
                                        if (typeof elHeader != "undefined") {
                                            eventbox.append('<h2>' + elHeader + '</h2>');
                                        }
                                        list = '<table class="qt_eventListTable"> ' +
                                        '<tr class="qt_eventListTable_Head"> ' +
                                        '<td style="width:150px;">Starting On</td> ' +
                                        '<td style="width:100px;">Time</td> ' +
                                        '<td style="width:180px;">Name</td> ' +
                                        '<td style="width:80px;"></td> ' +
                                        '</tr>';
                                    }
                                    if (typeof event === 'undefined') {
                                        //this check is needed for IE to prevent errrors
                                        return false;
                                    }
                                    else {

                                        var full = '';
                                        if (event.remainingCapacity == 0) {
                                            full = ' qt_eventListTable_Row-Full ';
                                        }
                                        list = list + '<tr class="qt_eventListTable_Row ' + full + '"  onclick="parent.location = \'http://duckrow.net/Classes/' + event.id + '\'" > ' +
                                            '<td>' + event.start + '</td> ' +
                                            '<td>' + event.time + '</td> ' +
                                            '<td>' + event.name + '</td> ' +
                                            '<td>Book Now</td> ' +
                                            '</tr> ';
                                        row++;
                                    }
                                    //if (i == 10) return false;
                                });

                                if (row == 0) {
                                    list = list + '<a href="http://duckrow.net/' + elCompany + '">http://duckrow.net/' + elCompany + '</a>';
                                }
                                else {
                                    list = list + '</table>';
                                    eventbox.append('<link href="http://duckrow.net/Widgets/style.css")" rel="Stylesheet" />');
                                }
                                eventbox.append(list);
                            }
                        });
                    }
                }
                else {
                    var wHTML = "";
                    wHTML += ('[Unable to connect to server]');
                    $(this).innerHTML = wHTML;
                    //document.getElementById('eventList').innerHTML = wHTML;
                }

            });

        }); // end each


    }



})();






