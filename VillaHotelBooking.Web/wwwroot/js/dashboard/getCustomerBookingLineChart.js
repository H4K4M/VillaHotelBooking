$(document).ready(function () {
    loadgetCustomerBookingLineChart();
});

function loadgetCustomerBookingLineChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetMemberAndBookingLineChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {
            

            loadLineChart("newMembersBookingsLineChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadLineChart(chartId, data) {
    var chartColors = getChartColorsArray(chartId);

    var options = {
        series: data.series,
        colors: chartColors,
        chart: {
            height: 350,
            type: 'line',
        },
        stroke: {
            show: true,
            curve: 'smooth',
            width: 3,
        },
        markers: {
            size: 3,
            strokeWidth: 0,
            hover: {
                size: 6
            }
        },
        xaxis: {
            categories: data.categories,
            labels: {
                style: {
                    colors: "#ddd"
                }
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: "#fff"
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: "#fff",
                useSeriesColors: true
            }
        },
        tooltip: {
            theme: 'dark',
        },
    }


    var chart = new ApexCharts(document.querySelector("#" + chartId), options);
    chart.render();
}

