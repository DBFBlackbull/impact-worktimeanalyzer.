$(function() {
    google.charts.load('current', { packages: ['corechart'] });
    google.charts.setOnLoadCallback(drawData);

    function drawData(json, graphTitle) {
        var data = new google.visualization.arrayToDataTable(json);

        var options = {
            width: 1600,
            height: 800,
            isStacked: true,
            title: graphTitle,
            vAxis: {
                title: 'Timer',
                ticks: [20, 22.5, 25, 27.5, 30, 32.5, 35, 37.5, 40, 42.5, 45, 47.5, 50],
                viewWindowMode: 'explicit',
                viewWindow: {
                    max: 50,
                    min: 20
                }
            },
            hAxis: {
                title: 'Uge'
            }
        };

		var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
        chart.draw(data, options);
	}

    function helloWorld() {
		window.alert('Hello world');
        console.log('Hello world');
    }
});

