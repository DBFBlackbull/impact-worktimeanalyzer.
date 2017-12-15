function drawGraphs(balance, weeks) {
    
    google.charts.load('current', { packages: ['corechart', 'bar'] });
    google.charts.setOnLoadCallback(function() { drawBalanceChart(balance), drawWeeksChart(weeks)});
}

function drawBalanceChart (balance) {
    var data = google.visualization.arrayToDataTable(balance.json);

    var options = {
        chart: {
            title: balance.title,
            subtitle: balance.subTitle
        },
        height: 150,
        colors: [balance.color],
        hAxis: {
            viewWindowMode: 'explicit',
            viewWindow: {
                max: 10,
                min: -10
            }
        },
        bars: 'horizontal' // Required for Material Bar Charts.
    };

    var chart = new google.charts.Bar(document.getElementById('balance_chart'));
    chart.draw(data, google.charts.Bar.convertOptions(options));
}

function drawWeeksChart(weeks) {
    var options = {
        width: 1600,
        height: 800,
        isStacked: true,
        title: weeks.graphTitle,
        colors: ['#289eff', '#3366cc', '#FF4635', 'orange', 'green'],
        animation:{
            duration: 1000,
            easing: 'out'
        },
        vAxis: {
            title: 'Timer',
            ticks: [2.5, 5, 7.5, 10, 12.5, 15, 17.5, 20, 22.5, 25, 27.5, 30, 32.5, 35, 37.5, 40, 42.5, 45, 47.5, 50, 52.5, 55],
            viewWindowMode: 'explicit',
            viewWindow: {
                max: 50,
                min: 0
            }
        },
        hAxis: {
            title: 'Uge'
        }
    };

    var newJson = weeks.isNormalized ? weeks.normalizedJson : weeks.json;
    var data = new google.visualization.arrayToDataTable(newJson);

    var chart = new google.visualization.ColumnChart(document.getElementById('weeks_chart'));
    chart.draw(data, options);

    $('#toggle-normalized').change(function() {
        var checked = $(this).prop('checked');

        var newJson = checked ? weeks.normalizedJson : weeks.json;
        var data = new google.visualization.arrayToDataTable(newJson);
        chart.draw(data, options);
    });
}