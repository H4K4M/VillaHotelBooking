$(document).ready(function () {
    loadTotalUserRadialChart();
});

function loadTotalUserRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetRegisteredUserRadialChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalUserCount").innerHTML = data.totalCount;

            var sectionCurrentCount = document.createElement("span");
            if (data.hasRatioIncrease) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML = `<i class="bi bi-arrow-up-right-circle me-1"></i><span> ${data.countInCurrentMonth} </span>`;
            }
            else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = `<i class="bi bi-arrow-down-right-circle me-1"></i><span> ${data.countInCurrentMonth} </span>`;
            }

            document.querySelector("#sectionUserCount").append(sectionCurrentCount);
            document.querySelector("#sectionUserCount").append("Since last month");

            loadRadialBarChart("totalUserRadialChart", data);

            $(".chart-spinner").hide();
        }
    });
}

