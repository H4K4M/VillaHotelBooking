function loadRadialBarChart(chartId, data) {
    var chartColors = getChartColorsArray(chartId);
    var options = {
        fill: {
            colors: chartColors
        },
        chart: {
            height: 90,
            width: 90,
            type: "radialBar",
            sparkline: {
                enabled: true
            },
            offsetY: -10
        },

        series: data.series,

        plotOptions: {
            radialBar: {
                dataLabels: {
                    value: {
                        offsetY: -10,
                        color: chartColors[0],
                    }
                }
            }
        },

        labels: [""]
    };

    var chart = new ApexCharts(document.querySelector("#" + chartId), options);

    chart.render();
}


function getChartColorsArray(chartId) {
    if (document.getElementById(chartId) != null) {
        var colors = document.getElementById(chartId).getAttribute("data-colors");
        if (colors) {
            colors = JSON.parse(colors);
            return colors.map(function (value) {
                var newValue = value.replace(" ", "");
                if (newValue.indexOf(",") === -1) {
                    var color = getComputedStyle(document.documentElement).getPropertyValue(newValue);
                    if (color) return color;
                    else return newValue;;
                }
            });
        }
    }
}