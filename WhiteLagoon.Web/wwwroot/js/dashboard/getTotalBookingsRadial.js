$(document).ready(function () {
    loadTotalBookingRadialChart();
});

function loadTotalBookingRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetTotalBookingRadialChartData", //Link to Json data 
        //returned by DashBoard/ GetTotalBookingRadialChartData endpoint
        type: 'GET', //GET is the default HTTP method of action
        dataType: 'json',
        success: function (data) {
            document
                .querySelector("#spanTotalBookingCount")
                .innerHTML = data.totalCount;

            var sectionCurrentCount = document.createElement("span");
            if (data.hasRatioIncreased) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML
                    = '<i class="bi bi-arrow-up-right-circle me-1"></i> </span> ' + data.countInCurrentMonth;
            }
            else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-down-right-circle me-1></i> </span> ' + data.countInCurrentMonth;
            }

            document.querySelector("#sectionBookingCount").append(sectionCurrentCount);
            document.querySelector("#sectionBookingCount").append("since last month");
            /*document.querySelector("#sectionBookingCount").innerHTML = sectionCurrentCount.outerHTML + " since last month";*/

            loadRadialBarChart("totalBookingsRadialChart", data);

            $(".chart-spinner").hide();
        }
    });
}