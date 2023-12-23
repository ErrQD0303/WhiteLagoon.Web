$(document).ready(function () {
    loadCustomerBookingRadialChart();
});

function loadCustomerBookingRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetBookingPieChartData",
        type: 'GET', //GET is the default HTTP method of action
        dataType: 'json',
        success: function (data) {

            loadPieChart("customerBookingPieChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadPieChart(id, data) {
    var chartColors = getChartColorsArray(id);
    options = {
        series: data.series,
        labels: data.labels, 
        colors: chartColors, 
        chart: {
            width: 380,
            type: 'pie'
        }, 
        stroke: {
            show: false
        },
        legend: {
            position: 'bottom', 
            horizontalAlign: "center", 
            labels: {
                colors: "#fff", 
                useSeriesColors: true
            }
        }
    }

    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}