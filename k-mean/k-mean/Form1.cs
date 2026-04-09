using Microsoft.VisualBasic;
using System.IO;

namespace k_mean
{
    public partial class MainForm : Form
    {
        private List<DataPoint> _points;
        private List<Centroid> _centroids;
        private List<Centroid> _initialCentroids;
        private int _k;
        private ToolTip _toolTip = new ToolTip();
        private System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private RichTextBox _workingsBox;
        private List<string> _pointNames = new List<string>();

        // Chart area boundaries
        private const int ChartLeft = 60;
        private const int ChartTop = 80;
        private const int ChartRight = 560;
        private const int ChartBottom = 580;
        private KMeans _kmeans;
        private int _currentIteration = 0;

        public MainForm()
        {
            InitializeComponent();
            _timer.Tick += Timer_Tick;
            this.Size = new Size(720, 680);
            this.Text = "K-Means Clustering";
            this.BackColor = Color.FromArgb(18, 18, 30);

            Button btnRun = new Button();
            btnRun.Text = "Run K-Means";
            btnRun.Size = new Size(150, 40);
            btnRun.Location = new Point(ChartLeft, 20);
            btnRun.BackColor = Color.FromArgb(99, 102, 241);
            btnRun.ForeColor = Color.White;
            btnRun.FlatStyle = FlatStyle.Flat;
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnRun.Cursor = Cursors.Hand;
            btnRun.Click += BtnRun_Click;
            this.Controls.Add(btnRun);

            Button btnStep = new Button();
            btnStep.Text = "Step";
            btnStep.Size = new Size(100, 40);
            btnStep.Location = new Point(220, 20);
            btnStep.BackColor = Color.FromArgb(34, 197, 94);
            btnStep.ForeColor = Color.White;
            btnStep.FlatStyle = FlatStyle.Flat;
            btnStep.FlatAppearance.BorderSize = 0;
            btnStep.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnStep.Cursor = Cursors.Hand;
            btnStep.Click += BtnStep_Click;
            this.Controls.Add(btnStep);

            Button btnReset = new Button();
            btnReset.Text = "Reset";
            btnReset.Size = new Size(100, 40);
            btnReset.Location = new Point(330, 20);
            btnReset.BackColor = Color.FromArgb(239, 68, 68);
            btnReset.ForeColor = Color.White;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnReset.Cursor = Cursors.Hand;
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Button btnAnimate = new Button();
            btnAnimate.Text = "Animate";
            btnAnimate.Size = new Size(100, 40);
            btnAnimate.Location = new Point(440, 20);
            btnAnimate.BackColor = Color.FromArgb(251, 191, 36);
            btnAnimate.ForeColor = Color.FromArgb(18, 18, 30);
            btnAnimate.FlatStyle = FlatStyle.Flat;
            btnAnimate.FlatAppearance.BorderSize = 0;
            btnAnimate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAnimate.Cursor = Cursors.Hand;
            btnAnimate.Click += BtnAnimate_Click;
            this.Controls.Add(btnAnimate);

            Button btnLoad = new Button();
            btnLoad.Text = "Load CSV";
            btnLoad.Size = new Size(100, 40);
            btnLoad.Location = new Point(550, 20);
            btnLoad.BackColor = Color.FromArgb(56, 189, 248);
            btnLoad.ForeColor = Color.FromArgb(18, 18, 30);
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLoad.Cursor = Cursors.Hand;
            btnLoad.Click += BtnLoad_Click;
            this.Controls.Add(btnLoad);

            _workingsBox = new RichTextBox();
            _workingsBox.Location = new Point(800, 80);
            _workingsBox.Size = new Size(550, 600);
            _workingsBox.BackColor = Color.FromArgb(28, 28, 45);
            _workingsBox.ForeColor = Color.FromArgb(200, 200, 220);
            _workingsBox.Font = new Font("Courier New", 8);
            _workingsBox.ReadOnly = true;
            _workingsBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.Controls.Add(_workingsBox);

            // make form taller to fit
            this.Size = new Size(1000, 720);
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("How many clusters? (1-5)", "K-Means Clustering", "3");
            if (string.IsNullOrWhiteSpace(input)) return;
            _k = int.Parse(input);

            if (_points == null)
                _points = DataGenerator.Generate(100);

            _kmeans = new KMeans(_points, _k);
            _currentIteration = 0;

            // ask if they want custom centroids
            DialogResult result = MessageBox.Show("Do you want to set custom starting centroids?",
                "Custom Centroids", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < _k; i++)
                {
                    string cx = Interaction.InputBox($"Centroid {i + 1} X value (0-1 for CSV data):", "Centroid X", "0.5");
                    string cy = Interaction.InputBox($"Centroid {i + 1} Y value (0-1 for CSV data):", "Centroid Y", "0.5");

                    if (string.IsNullOrWhiteSpace(cx) || string.IsNullOrWhiteSpace(cy)) return;

                    _kmeans.SetCentroid(i,
                        double.Parse(cx, System.Globalization.CultureInfo.InvariantCulture) * 500,
                        double.Parse(cy, System.Globalization.CultureInfo.InvariantCulture) * 500);
                }
            }

            _initialCentroids = _kmeans.Centroids
    .Select(c => new Centroid { X = c.X, Y = c.Y }).ToList();

            _kmeans.AssignClusters();
            _centroids = _kmeans.Centroids;
            _points = _kmeans.Points;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int chartW = ChartRight - ChartLeft;
            int chartH = ChartBottom - ChartTop;

            // --- Draw chart background ---
            g.FillRectangle(new SolidBrush(Color.FromArgb(28, 28, 45)), ChartLeft, ChartTop, chartW, chartH);

            // --- Draw grid lines ---
            Pen gridPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1);
            int gridCount = 5;
            for (int i = 1; i < gridCount; i++)
            {
                int x = ChartLeft + (chartW * i / gridCount);
                int y = ChartTop + (chartH * i / gridCount);
                g.DrawLine(gridPen, x, ChartTop, x, ChartBottom);
                g.DrawLine(gridPen, ChartLeft, y, ChartRight, y);
            }

            // --- Draw axes ---
            Pen axisPen = new Pen(Color.FromArgb(180, 180, 200), 2);
            g.DrawLine(axisPen, ChartLeft, ChartBottom, ChartRight, ChartBottom); // X axis
            g.DrawLine(axisPen, ChartLeft, ChartTop, ChartLeft, ChartBottom);     // Y axis

            // --- Axis labels ---
            Font labelFont = new Font("Segoe UI", 8);
            Brush labelBrush = new SolidBrush(Color.FromArgb(180, 180, 200));
            for (int i = 0; i <= gridCount; i++)
            {
                double val = i * 0.2; // 0.0, 0.2, 0.4, 0.6, 0.8, 1.0
                int x = ChartLeft + (chartW * i / gridCount);
                int y = ChartBottom - (chartH * i / gridCount);
                g.DrawString(val.ToString("F1"), labelFont, labelBrush, x - 10, ChartBottom + 5);
                g.DrawString(val.ToString("F1"), labelFont, labelBrush, ChartLeft - 35, y - 7);
            }

            // --- Axis titles ---
            Font axisFont = new Font("Segoe UI", 9, FontStyle.Bold);
            Brush axisBrush = new SolidBrush(Color.FromArgb(200, 200, 220));
            g.DrawString("X Axis", axisFont, axisBrush, ChartLeft + chartW / 2 - 20, ChartBottom + 25);

            // Rotated Y label
            g.TranslateTransform(15, ChartTop + chartH / 2);
            g.RotateTransform(-90);
            g.DrawString("Y Axis", axisFont, axisBrush, -20, 0);
            g.ResetTransform();

            string iterText = $"Iteration: {_currentIteration}";
            g.DrawString(iterText, new Font("Segoe UI", 10, FontStyle.Bold),
                new SolidBrush(Color.FromArgb(200, 200, 220)),
                ChartRight - 130, ChartTop + 10);

            if (_points == null) return;

            Color[] colors = {
                Color.FromArgb(239, 68, 68),
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(34, 197, 94),
                Color.FromArgb(251, 191, 36),
                Color.FromArgb(168, 85, 247)
            };

            // --- Draw data points ---
            foreach (DataPoint p in _points)
            {
                if (p.Cluster < 0) continue; // skip unassigned points
                Color c = colors[p.Cluster % colors.Length];
                float px = ChartLeft + (float)(p.X / 500.0 * chartW) - 4;
                float py = ChartBottom - (float)(p.Y / 500.0 * chartH) - 4;
                g.FillEllipse(new SolidBrush(Color.FromArgb(180, c)), px, py, 8, 8);
            }

            // --- Draw centroids ---
            if (_centroids != null)
            {
                for (int i = 0; i < _centroids.Count; i++)
                {
                    Color c = colors[i % colors.Length];
                    float cx = ChartLeft + (float)(_centroids[i].X / 500.0 * chartW) - 10;
                    float cy = ChartBottom - (float)(_centroids[i].Y / 500.0 * chartH) - 10;
                    g.FillEllipse(new SolidBrush(c), cx, cy, 20, 20);
                    g.DrawEllipse(new Pen(Color.White, 2), cx, cy, 20, 20);

                    // Centroid label
                    g.DrawString($"C{i + 1}", new Font("Segoe UI", 7, FontStyle.Bold),
                        Brushes.White, cx + 4, cy + 4);
                }

                int legendX = ChartRight + 10;
                int legendY = ChartTop;

                g.DrawString("Clusters", new Font("Segoe UI", 9, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(200, 200, 220)), legendX, legendY);

                for (int i = 0; i < _centroids.Count; i++)
                {
                    Color c = colors[i % colors.Length];
                    int ly = legendY + 25 + (i * 25);
                    g.FillEllipse(new SolidBrush(c), legendX, ly, 12, 12);
                    g.DrawString($"Cluster {i + 1}", new Font("Segoe UI", 8),
                        new SolidBrush(Color.FromArgb(200, 200, 220)), legendX + 18, ly);
                }

                for (int i = 0; i < _centroids.Count; i++)
                {
                    int clusterSize = _points.Count(p => p.Cluster == i);
                    int ly = legendY + 25 + (i * 25);
                    g.DrawString($"({clusterSize} pts)", new Font("Segoe UI", 7),
                        new SolidBrush(Color.FromArgb(150, 150, 170)),
                        legendX + 18, ly + 13);
                }
            }
         }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_centroids == null) return;

            int chartW = ChartRight - ChartLeft;
            int chartH = ChartBottom - ChartTop;

            for (int i = 0; i < _centroids.Count; i++)
            {
                float cx = ChartLeft + (float)(_centroids[i].X / 500.0 * chartW) - 10;
                float cy = ChartBottom - (float)(_centroids[i].Y / 500.0 * chartH) - 10;

                if (e.X >= cx && e.X <= cx + 20 && e.Y >= cy && e.Y <= cy + 20)
                {
                    _toolTip.SetToolTip(this, $"Centroid {i + 1}  |  X = {_centroids[i].X / 500:F2},  Y = {_centroids[i].Y / 500:F2}");
                    return;
                }
            }
            _toolTip.SetToolTip(this, "");
        }


        private void BtnStep_Click(object sender, EventArgs e)
        {
            if (_kmeans == null) return;
            List<Centroid> oldCentroids = _kmeans.Centroids
                .Select(c => new Centroid { X = c.X, Y = c.Y }).ToList();
            _kmeans.StepAndLog(_currentIteration + 1);
            _currentIteration++;
            _centroids = _kmeans.Centroids;
            _points = _kmeans.Points;
            UpdateWorkings();
            this.Invalidate();
            if (_kmeans.HasConverged(oldCentroids))
                MessageBox.Show($"Converged after {_currentIteration} iterations!",
                    "Optimal Solution Reached", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _points = null;
            _centroids = null;
            _kmeans = null;
            _initialCentroids = null;
            _currentIteration = 0;
            this.Invalidate();
        }

        private void BtnAnimate_Click(object sender, EventArgs e)
        {
            if (_kmeans == null) return;
            _timer.Interval = 500; // half second between steps
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            List<Centroid> oldCentroids = _kmeans.Centroids
                .Select(c => new Centroid { X = c.X, Y = c.Y }).ToList();
            _kmeans.StepAndLog(_currentIteration + 1);
            _currentIteration++;
            _centroids = _kmeans.Centroids;
            _points = _kmeans.Points;
            UpdateWorkings();
            this.Invalidate();
            if (_kmeans.HasConverged(oldCentroids))
            {
                _timer.Stop();
                MessageBox.Show($"Converged after {_currentIteration} iterations!",
                    "Optimal Solution Reached", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV Files (*.csv)|*.csv";
            dialog.Title = "Select a CSV File";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _points = new List<DataPoint>();
                string[] lines = File.ReadAllLines(dialog.FileName);

                foreach (string line in lines.Skip(1)) // skip header row
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        DataPoint p = new DataPoint();
                        p.X = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture) * 500;
                        p.Y = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) * 500;
                        _points.Add(p);
                    }
                }

                MessageBox.Show($"Loaded {_points.Count} points!", "CSV Loaded",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Invalidate();
            }
        }

        private void UpdateWorkings()
        {
            _workingsBox.Clear();
            if (_kmeans == null || _points == null) return;

            // --- Initial data table ---
            _workingsBox.AppendText("=== INITIAL DATA ===\r\n");
            _workingsBox.AppendText("     ");
            for (int i = 0; i < _points.Count; i++)
                _workingsBox.AppendText($"  a{i + 1,-6}");
            _workingsBox.AppendText("\r\n");

            _workingsBox.AppendText("X1   ");
            foreach (DataPoint p in _points)
                _workingsBox.AppendText($"  {p.X / 500.0:F2}  ");
            _workingsBox.AppendText("\r\n");

            _workingsBox.AppendText("X2   ");
            foreach (DataPoint p in _points)
                _workingsBox.AppendText($"  {p.Y / 500.0:F2}  ");
            _workingsBox.AppendText("\r\n\r\n");

            // --- Initial centroids ---
            _workingsBox.AppendText("=== INITIAL CENTROIDS ===\r\n");
            if (_initialCentroids != null)
            {
                for (int i = 0; i < _initialCentroids.Count; i++)
                    _workingsBox.AppendText($"C{i + 1}: X1={_initialCentroids[i].X / 500.0:F2}, X2={_initialCentroids[i].Y / 500.0:F2}\r\n");
            }
            _workingsBox.AppendText("\r\n");

            // --- Each iteration ---
            foreach (IterationLog log in _kmeans.Log)
            {
                _workingsBox.AppendText($"=== ITERATION {log.Iteration} ===\r\n");

                // D values
                for (int j = 0; j < log.NewCentroids.Count; j++)
                {
                    _workingsBox.AppendText($"D{j}=  ");
                    for (int i = 0; i < _points.Count; i++)
                        _workingsBox.AppendText($"  {log.Distances[i, j]:F4}");
                    _workingsBox.AppendText("\r\n");
                }
                _workingsBox.AppendText("\r\n");

                // G values
                _workingsBox.AppendText("G =   ");
                for (int i = 0; i < log.Assignments.Length; i++)
                    _workingsBox.AppendText($"  {log.Assignments[i] + 1}      ");
                _workingsBox.AppendText("\r\n\r\n");

                // Groups
                for (int j = 0; j < log.NewCentroids.Count; j++)
                {
                    var group = log.Assignments
                        .Select((cluster, idx) => new { cluster, idx })
                        .Where(x => x.cluster == j)
                        .Select(x => $"a{x.idx + 1}");
                    _workingsBox.AppendText($"Group {j + 1} = {{ {string.Join(", ", group)} }}\r\n");
                }
                _workingsBox.AppendText("\r\n");

                // New centroids
                _workingsBox.AppendText("New Centroids:\r\n");
                for (int j = 0; j < log.NewCentroids.Count; j++)
                    _workingsBox.AppendText($"C{j + 1}: X1={log.NewCentroids[j].X / 500.0:F2}, X2={log.NewCentroids[j].Y / 500.0:F2}\r\n");
                _workingsBox.AppendText("\r\n");
            }

            // --- Convergence message ---
            if (_kmeans.Log.Count >= 2)
            {
                var last = _kmeans.Log[_kmeans.Log.Count - 1];
                var prev = _kmeans.Log[_kmeans.Log.Count - 2];
                if (_kmeans.HasConverged(prev.NewCentroids))
                    _workingsBox.AppendText($"G{prev.Iteration} = G{last.Iteration} => CONVERGED!\r\n");
            }
        }
    }
}
