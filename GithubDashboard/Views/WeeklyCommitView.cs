using System;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using ShinobiCharts;
using System.Collections.Generic;
using GithubAPI;
using System.Drawing;
using GithubDashboard.Utilities;

namespace GithubDashboard
{
	[Register("WeeklyCommitView")]
	public class WeeklyCommitView : LoadingIndicatorView, IDataView<WeeklyCommitData>
	{
		private class WeeklyCommitViewDatasource : SChartDataSource
		{
			private IList<SChartDataPoint> _ownerDPs;
			private IList<SChartDataPoint> _nonOwnerDPs;

			public WeeklyCommitViewDatasource(WeeklyCommitData data)
			{
				if(data != null) {
					_ownerDPs = this.ConvertToDataPoints(data.Owner);
					_nonOwnerDPs = this.ConvertToDataPoints(data.Others);
				} else {
					_ownerDPs = new List<SChartDataPoint> ();
					_nonOwnerDPs = new List<SChartDataPoint>();
				}
			}

			#region Utility Methods
			private IList<SChartDataPoint>ConvertToDataPoints(IEnumerable<int> inp)
			{
				if (inp == null) {
					return new List<SChartDataPoint> ();
				} else {
					return new List<SChartDataPoint> (
						inp.Select ((x, i) => this.CreateDataPoint (i, x))
					);
				}
			}

			private SChartDataPoint CreateDataPoint(int x, int y)
			{
				SChartDataPoint dp = new SChartDataPoint ();
				dp.XValue = (NSNumber)x;
				dp.YValue = (NSNumber)y;
				return dp;
			}
			#endregion

			#region SChartDatasource Protocol Methods
			public override int GetNumberOfSeries (ShinobiChart chart)
			{
				return 2;
			}

			public override int GetNumberOfDataPoints (ShinobiChart chart, int dataSeriesIndex)
			{
				return _ownerDPs.Count ();
			}

			public override SChartData GetDataPoint (ShinobiChart chart, int dataIndex, int dataSeriesIndex)
			{
				if (dataSeriesIndex == 0) {
					return _nonOwnerDPs [dataIndex];
				} else {
					return _ownerDPs [dataIndex];
				}
			}

			public override SChartSeries GetSeries (ShinobiChart chart, int dataSeriesIndex)
			{
				SChartColumnSeries series = new SChartColumnSeries ();
				series.StackIndex = 0;
				if (dataSeriesIndex == 0) {
					series.Title = "Non-owner";
				} else {
					series.Title = "Owner";
				}
				return series;
			}
			#endregion
		}


		private ShinobiChart _columnChart;
		private WeeklyCommitViewDatasource _dataSource;

		public WeeklyCommitView (IntPtr p) : base(p)
		{
		}

		public void RenderData(WeeklyCommitData data)
		{
			_dataSource = new WeeklyCommitViewDatasource(data);

			// If we haven't got a chart, then create one
			if(_columnChart == null)
			{
				this.CreateChart ();
			}

			// Assign it to this chart
			_columnChart.DataSource = _dataSource;

			// And then redraw the chart
			_columnChart.RedrawChart();

			// Get rid of the activity indicator
			HideIndicator ();
		}

		private void CreateChart()
		{
			_columnChart = new ShinobiChart (this.Bounds);
			_columnChart.LicenseKey = ShinobiLicenseKeyProviderJson.Instance.ChartsLicenseKey;
			_columnChart.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

			SChartNumberAxis xAxis = new SChartNumberAxis ();
			xAxis.Title = "Week";
			_columnChart.XAxis = xAxis;

			SChartNumberAxis yAxis = new SChartNumberAxis ();
			yAxis.RangePaddingHigh = (NSNumber)0.5;
			yAxis.Title = "Commits";
			_columnChart.YAxis = yAxis;

			// Display the legend
			_columnChart.Legend.Hidden = false;
			_columnChart.Legend.Placement = SChartLegendPlacement.InsidePlotArea;
			_columnChart.Legend.Position = SChartLegendPosition.TopLeft;

			// disable interaction - in order to allow paging of the container view
			_columnChart.UserInteractionEnabled = false;

			// Add it as a subview
			this.Add (_columnChart);
		}
	}
}

