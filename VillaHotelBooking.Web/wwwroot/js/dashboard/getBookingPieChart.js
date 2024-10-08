$(document).ready(function () {
    loadgetBookingPieChart();
});

function loadgetBookingPieChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetBookinPieChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {
            

            loadPieChart("customerBookingsPieChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadPieChart(chartId, data) {
    var chartColors = getChartColorsArray(chartId);

    var options = {
        series: data.series,
        labels: data.labels,
        colors: chartColors,
        chart: {
            type: 'pie',
            width: 380
        },
        stroke: {
            show: false,
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: "#fff",
                useSeriesColors: true
            }
        },
    }


    var chart = new ApexCharts(document.querySelector("#" + chartId), options);
    chart.render();
}

