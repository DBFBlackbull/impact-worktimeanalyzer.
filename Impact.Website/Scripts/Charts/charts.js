function drawMaterialBarOrColumnChart (chartModel) {
    var data = getDataToDraw(chartModel);
    var options = google.charts.Bar.convertOptions(chartModel.options);
    
    var chart = new google.charts.Bar(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function drawColumnChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.ColumnChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function getDataToDraw(chartModel) {
    var showNormalized = $('#toggle-normalized').prop('checked');
    var showAllWeeks = $('#toggle-allWeeks').prop('checked');

    var json;
    if (!showNormalized && chartModel.rawWeeks !== undefined)
        json = chartModel.rawWeeks;
    else if (showAllWeeks)
        json = chartModel.normalizedAllWeeks;
    else
        json = chartModel.normalizedPreviousWeeks;
    
    return new google.visualization.arrayToDataTable(json);
}

function drawMaterialOverviewChart(months) {
    var newJson = months.isNormalized ? months.normalizedJson : months.json;
    var data = new google.visualization.arrayToDataTable(newJson);
    var options = google.charts.Bar.convertOptions(months.options);

    var chart = new google.charts.Bar(document.getElementById(months.divId));
    chart.draw(data, options);

    $('#toggle-normalized').change(function() {
        var checked = $(this).prop('checked');

        var newJson = checked ? months.normalizedJson : months.json;
        var data = new google.visualization.arrayToDataTable(newJson);
        chart.draw(data, options);
    });
}

function drawPieChart(pie) {
    var data = google.visualization.arrayToDataTable(pie.previousWeeks);
    var options = pie.options;

    var chart = new google.visualization.PieChart(document.getElementById(pie.divId));
    chart.draw(data, options);
    
    $('#toggle-allWeeks').change(function() {
        var checked = $(this).prop('checked');

        var newData = checked ? pie.allWeeks : pie.previousWeeks;
        var data = new google.visualization.arrayToDataTable(newData);
        chart.draw(data, options);
    });
}

function drawGaugeChart(gauge) {
    var data = google.visualization.arrayToDataTable(gauge.previousWeeks);
    var options = gauge.options;

    var chart = new google.visualization.Gauge(document.getElementById(gauge.divId));
    chart.draw(data, options);

    $('#toggle-allWeeks').change(function() {
        var checked = $(this).prop('checked');

        var newData = checked ? gauge.allWeeks : gauge.previousWeeks;
        var data = new google.visualization.arrayToDataTable(newData);
        chart.draw(data, options);
    });
}