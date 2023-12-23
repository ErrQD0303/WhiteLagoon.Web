$(document).ready(function () {
    loadRevenueRadialChart();
});

function loadRevenueRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetRevenueChartData", //Link to Json data 
        //returned by DashBoard/ GetRevenueChartData endpoint
        type: 'GET', //GET is the default HTTP method of action
        dataType: 'json',
        success: function (data) {
            document
                .querySelector("#spanTotalRevenueCount")
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

            document.querySelector("#sectionRevenueCount").append(sectionCurrentCount);
            document.querySelector("#sectionRevenueCount").append("since last month");
            /*document.querySelector("#sectionRevenueCount").innerHTML = sectionCurrentCount.outerHTML + " since last month";*/

            loadRadialBarChart("totalRevenueRadialChart", data);

            $(".chart-spinner").hide();
        }
    });
}