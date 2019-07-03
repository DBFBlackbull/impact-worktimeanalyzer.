function drawColumnChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.ColumnChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function drawMaterialBarOrColumnChart (chartModel) {
    var data = getDataToDraw(chartModel);
    var options = google.charts.Bar.convertOptions(chartModel.options);

    var chart = new google.charts.Bar(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function drawPieChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.PieChart(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function drawGaugeChart(chartModel) {
    var data = getDataToDraw(chartModel);
    var options = chartModel.options;

    var chart = new google.visualization.Gauge(document.getElementById(chartModel.divId));
    chart.draw(data, options);

    onToggleChange(chart, chartModel, options);
}

function onToggleChange(chart, chartModel, options) {
    $('#toggle-normalized').change(function () {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
    $('#toggle-allWeeks').change(function() {
        var data = getDataToDraw(chartModel);
        chart.draw(data, options);
    });
}

function getDataToDraw(chartModel) {
    var json = getRawData(chartModel); 
    return new google.visualization.arrayToDataTable(json);
}

function getRawData(chartModel) {
    var showNormalized = $('#toggle-normalized').prop('checked');
    var showAllWeeks = $('#toggle-allWeeks').prop('checked');

    var rawDataDefined = chartModel.rawData !== undefined;
    var normalizedPreviousDataDefined = chartModel.normalizedPreviousData !== undefined;
    var normalizedAllDataDefined = chartModel.normalizedAllData !== undefined;
    
    //Only raw defined
    if (!normalizedPreviousDataDefined && !normalizedAllDataDefined) {
        return chartModel.rawData;
    }

    //Only previous defined
    if (!rawDataDefined && !normalizedAllDataDefined) {
        return chartModel.normalizedPreviousData;
    }
    
    //Only normalized all defined
    if (!rawDataDefined && !normalizedPreviousDataDefined) {
        return chartModel.normalizedAllData;
    }
    
    //All data sets defined
    if (rawDataDefined && normalizedPreviousDataDefined && normalizedAllDataDefined) {
        if (showNormalized && showAllWeeks) {
            return chartModel.normalizedAllData;
        }
        if (showNormalized) {
            return chartModel.normalizedPreviousData;
        }
        return chartModel.rawData;
    }
    
    // normalized previous and all are defined
    if (!rawDataDefined) {
        if (showNormalized && showAllWeeks) {
            return chartModel.normalizedAllData;
        }
        return chartModel.normalizedPreviousData;
    }
    
    // raw and normalized all are defined
    if (!normalizedPreviousDataDefined) {
        if (showNormalized) {
            return chartModel.normalizedAllData;
        }
        return chartModel.rawData;
    }
    
    console.log('No dataset could be found. Does the model only have raw and normalized previous?')
}