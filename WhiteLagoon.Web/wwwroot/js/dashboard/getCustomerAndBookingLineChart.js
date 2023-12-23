$(document).ready(function () {
    loadCustomerBookingLineChart();
});

function loadCustomerBookingLineChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetMemberAndBookingLineChartData",
        type: 'GET', //GET is the default HTTP method of action
        dataType: 'json',
        success: function (data) {

            loadLineChart("newMembersAndBookingsLineChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadLineChart(id, data) {
    var chartColors = getChartColorsArray(id);
    options = {
        colors: chartColors, 
        series: data.series, 
        chart: {
            height: 350,
            type: 'line'
        }, 
        stroke: {
            curve: 'smooth', 
            width: 2
        },
        markers: {
            size: 3, 
            strokeWidth: 0,
            hover: {
                size: 7
            }
        },
        xaxis: {
            categories: data.categories
        }, 
        yaxis: {
            labels: {
                style: {
                    colors: "#fff"
                }
            }
        }, 
        legend: {
            labels: {
                colors: "#fff", 
                useSeriesColors: true
            }
        }, 
        tooltip: {
            theme: 'dark'
        }
    }

    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}