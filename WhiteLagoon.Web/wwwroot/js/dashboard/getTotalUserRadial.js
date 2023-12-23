$(document).ready(function () {
    loadUserRadialChart();
});

function loadUserRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetRegisteredUserChartData", //Link to Json data 
        //returned by DashBoard/ GetUserRadialChartData endpoint
        type: 'GET', //GET is the default HTTP method of action
        dataType: 'json',
        success: function (data) {
            document
                .querySelector("#spanTotalUserCount")
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

            document.querySelector("#sectionUserCount").append(sectionCurrentCount);
            document.querySelector("#sectionUserCount").append("since last month");
            /*document.querySelector("#sectionUserCount").innerHTML = sectionCurrentCount.outerHTML + " since last month";*/

            loadRadialBarChart("totalUserRadialChart", data);

            $(".chart-spinner").hide();
        }
    });
}