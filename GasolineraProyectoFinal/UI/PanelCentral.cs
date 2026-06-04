using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GasolineraSimulacion.Models;
using GasolineraSimulacion.Services;

namespace GasolineraSimulacion.UI
{
    public partial class PanelCentral : Form
    {
        private readonly RegistroService _registro;
        private readonly ArduinoService _arduino;
        private readonly EstadisticasService _estadisticas;

        private readonly List<Bomba> _bombas = new List<Bomba>
        {
            new Bomba(1), new Bomba(2), new Bomba(3), new Bomba(4)
        };

        private Panel[] panelBombas = new Panel[4];
        private Label[] lblEstadoBomba = new Label[4];
        private Button[] btnDetener = new Button[4];

        private TextBox txtCliente;
        private ComboBox cmbBomba, cmbTipo;
        private NumericUpDown numMonto;
        private Label lblPrecio, lblLitrosCalc;
        private Button btnConfirmar;

        private DataGridView dgvHistorial;
        private DateTimePicker dtpCierre;
        private RichTextBox rtbEstadisticas;

        private ComboBox cmbPuerto;
        private NumericUpDown numBaud;
        private Button btnConectar, btnDesconectar, btnRefrescarPuertos;
        private RichTextBox rtbLog;
        private Label lblEstadoConexion;

        private NumericUpDown numPrecio;
        private Label lblPrecioActual;

        private TabControl tabControl;
        private TabPage tabPanel, tabHistorial, tabEstadisticas, tabConexion;

        public PanelCentral()
        {
            _registro = new RegistroService();
            _arduino = new ArduinoService();
            _estadisticas = new EstadisticasService(_registro);

            InitializeComponent();
            SuscribirEventosArduino();
            ActualizarEstadoBombas();
            ActualizarPrecioLabel();
        }

        private void InitializeComponent()
        {
            this.Text = "Panel Central – Gasolinera Simulación";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(1050, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(200, 36),
                SizeMode = TabSizeMode.Fixed
            };

            tabPanel = new TabPage("🛢  Panel Principal") { BackColor = Color.FromArgb(35, 35, 35) };
            tabHistorial = new TabPage("📋  Historial") { BackColor = Color.FromArgb(35, 35, 35) };
            tabEstadisticas = new TabPage("📊  Estadísticas") { BackColor = Color.FromArgb(35, 35, 35) };
            tabConexion = new TabPage("🔌  Arduino") { BackColor = Color.FromArgb(35, 35, 35) };

            tabControl.TabPages.AddRange(new[] { tabPanel, tabHistorial, tabEstadisticas, tabConexion });
            this.Controls.Add(tabControl);

            ConstruirTabPanel();
            ConstruirTabHistorial();
            ConstruirTabEstadisticas();
            ConstruirTabConexion();
        }

        // ─────────────────────────────────────────────
        // TAB: PANEL PRINCIPAL
        // ─────────────────────────────────────────────
        private void ConstruirTabPanel()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ── IZQUIERDA ──
            var leftContainer = new Panel { Dock = DockStyle.Fill };

            var grpBombas = CrearGroupBox("Estado de Bombas");
            grpBombas.Dock = DockStyle.Fill;

            var bombLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(6)
            };
            bombLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            bombLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            bombLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            bombLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            for (int i = 0; i < 4; i++)
            {
                panelBombas[i] = new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(4),
                    BackColor = Color.FromArgb(35, 60, 35),
                    BorderStyle = BorderStyle.FixedSingle
                };
                lblEstadoBomba[i] = new Label
                {
                    Text = $"BOMBA {i + 1}\nDISPONIBLE",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.LimeGreen
                };
                btnDetener[i] = new Button
                {
                    Text = "■ Detener",
                    Dock = DockStyle.Bottom,
                    Height = 32,
                    BackColor = Color.FromArgb(160, 40, 40),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Enabled = false,
                    Tag = i + 1,
                    Cursor = Cursors.Hand
                };
                btnDetener[i].FlatAppearance.BorderSize = 0;
                btnDetener[i].Click += BtnDetener_Click;
                panelBombas[i].Controls.Add(lblEstadoBomba[i]);
                panelBombas[i].Controls.Add(btnDetener[i]);
                bombLayout.Controls.Add(panelBombas[i], i % 2, i / 2);
            }
            grpBombas.Controls.Add(bombLayout);

            var grpPrecio = CrearGroupBox("Precio del Día (Q/Litro)");
            grpPrecio.Dock = DockStyle.Bottom;
            grpPrecio.Height = 80;
            var precioFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 4) };
            lblPrecioActual = new Label
            {
                Text = $"Precio actual: Q{_registro.ObtenerPrecioPorLitro():F2}/L",
                AutoSize = true,
                ForeColor = Color.LightYellow,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 6, 10, 0)
            };
            numPrecio = new NumericUpDown
            {
                Width = 100,
                Minimum = 0.01m,
                Maximum = 9999.99m,
                DecimalPlaces = 2,
                Increment = 0.5m,
                Value = (decimal)_registro.ObtenerPrecioPorLitro(),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Margin = new Padding(0, 4, 8, 0)
            };
            var btnGuardarPrecio = CrearBoton("Guardar Precio", Color.FromArgb(60, 120, 70));
            btnGuardarPrecio.Click += (s, e) =>
            {
                _registro.GuardarPrecioPorLitro((double)numPrecio.Value);
                ActualizarPrecioLabel();
                MessageBox.Show("Precio actualizado.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            precioFlow.Controls.AddRange(new Control[] { lblPrecioActual, numPrecio, btnGuardarPrecio });
            grpPrecio.Controls.Add(precioFlow);

            leftContainer.Controls.Add(grpBombas);
            leftContainer.Controls.Add(grpPrecio);

            // ── DERECHA: formulario ──
            var grpForm = CrearGroupBox("Nuevo Abastecimiento");
            grpForm.Dock = DockStyle.Fill;

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(14, 10, 14, 10)
            };
            for (int i = 0; i < 9; i++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140f));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            formLayout.Controls.Add(CrearLabel("Cliente:"), 0, 0);
            txtCliente = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            formLayout.Controls.Add(txtCliente, 1, 0);

            formLayout.Controls.Add(CrearLabel("Bomba:"), 0, 1);
            cmbBomba = new ComboBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbBomba.Items.AddRange(new object[] { "Bomba 1", "Bomba 2", "Bomba 3", "Bomba 4" });
            cmbBomba.SelectedIndex = 0;
            formLayout.Controls.Add(cmbBomba, 1, 1);

            formLayout.Controls.Add(CrearLabel("Tipo de pago:"), 0, 2);
            cmbTipo = new ComboBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipo.Items.AddRange(new object[] { "Prepago", "Tanque Lleno" });
            cmbTipo.SelectedIndex = 0;
            cmbTipo.SelectedIndexChanged += (s, e) =>
            {
                bool esPrepago = cmbTipo.SelectedIndex == 0;
                numMonto.Enabled = esPrepago;
                lblLitrosCalc.Visible = esPrepago;
                ActualizarLitrosCalculados();
            };
            formLayout.Controls.Add(cmbTipo, 1, 2);

            formLayout.Controls.Add(CrearLabel("Monto (Q):"), 0, 3);
            numMonto = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0.01m,
                Maximum = 99999.99m,
                DecimalPlaces = 2,
                Increment = 5m,
                Value = 50m,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White
            };
            numMonto.ValueChanged += (s, e) => ActualizarLitrosCalculados();
            formLayout.Controls.Add(numMonto, 1, 3);

            formLayout.Controls.Add(CrearLabel("Litros a servir:"), 0, 4);
            lblLitrosCalc = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            formLayout.Controls.Add(lblLitrosCalc, 1, 4);

            formLayout.Controls.Add(CrearLabel("Precio/Litro:"), 0, 5);
            lblPrecio = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.LightYellow,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f)
            };
            formLayout.Controls.Add(lblPrecio, 1, 5);

            btnConfirmar = CrearBoton("▶  Iniciar Abastecimiento", Color.FromArgb(25, 110, 190));
            btnConfirmar.Dock = DockStyle.Fill;
            btnConfirmar.Height = 44;
            btnConfirmar.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnConfirmar.Click += BtnConfirmar_Click;
            formLayout.SetColumnSpan(btnConfirmar, 2);
            formLayout.Controls.Add(btnConfirmar, 0, 7);

            grpForm.Controls.Add(formLayout);
            layout.Controls.Add(leftContainer, 0, 0);
            layout.Controls.Add(grpForm, 1, 0);
            tabPanel.Controls.Add(layout);
            ActualizarLitrosCalculados();
        }

        // ─────────────────────────────────────────────
        // TAB: HISTORIAL
        // ─────────────────────────────────────────────
        private void ConstruirTabHistorial()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10, 8, 10, 6) };
            var btnRefrescar = CrearBoton("🔄  Refrescar", Color.FromArgb(60, 90, 130));
            btnRefrescar.Click += (s, e) => CargarHistorial();
            topPanel.Controls.Add(btnRefrescar);

            dgvHistorial = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(65, 65, 65),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgvHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvHistorial.DefaultCellStyle.BackColor = Color.FromArgb(42, 42, 42);
            dgvHistorial.DefaultCellStyle.ForeColor = Color.White;
            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(52, 52, 52);

            tabHistorial.Controls.Add(dgvHistorial);
            tabHistorial.Controls.Add(topPanel);
            CargarHistorial();
        }

        private void CargarHistorial()
        {
            dgvHistorial.Columns.Clear();
            dgvHistorial.Rows.Clear();
            dgvHistorial.Columns.Add("Id", "ID");
            dgvHistorial.Columns.Add("Fecha", "Fecha/Hora");
            dgvHistorial.Columns.Add("Bomba", "Bomba");
            dgvHistorial.Columns.Add("Cliente", "Cliente");
            dgvHistorial.Columns.Add("Tipo", "Tipo");
            dgvHistorial.Columns.Add("Litros", "Litros Servidos");
            dgvHistorial.Columns.Add("Monto", "Monto (Q)");
            dgvHistorial.Columns.Add("Estado", "Estado");

            foreach (var a in _registro.ObtenerTodos().OrderByDescending(x => x.FechaHora))
            {
                dgvHistorial.Rows.Add(
                    a.Id,
                    a.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    $"Bomba {a.NumeroBomba}",
                    a.NombreCliente,
                    a.Tipo.ToString(),
                    $"{a.LitrosServidos:F3} L",
                    $"Q {a.MontoFinal:F2}",
                    a.Completado ? "✔ Completado" : "✘ Incompleto"
                );
            }
        }

        // ─────────────────────────────────────────────
        // TAB: ESTADÍSTICAS
        // ─────────────────────────────────────────────
        private void ConstruirTabEstadisticas()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill };

            dtpCierre = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            var btnCierre = CrearBoton("📅 Cierre Diario", Color.FromArgb(60, 90, 130));
            var btnPrepago = CrearBoton("💳 Informe Prepago", Color.FromArgb(70, 100, 55));
            var btnTanque = CrearBoton("⛽ Informe Tanque Lleno", Color.FromArgb(100, 75, 40));
            var btnBombas = CrearBoton("📊 Uso de Bombas", Color.FromArgb(85, 55, 100));
            var btnResumen = CrearBoton("🔎 Resumen General", Color.FromArgb(75, 75, 75));

            btnCierre.Click += (s, e) => MostrarCierreDiario();
            btnPrepago.Click += (s, e) => MostrarInformePrepago();
            btnTanque.Click += (s, e) => MostrarInformeTanqueLleno();
            btnBombas.Click += (s, e) => MostrarInformeBombas();
            btnResumen.Click += (s, e) => MostrarResumenGeneral();

            flow.Controls.AddRange(new Control[] { dtpCierre, btnCierre, btnPrepago, btnTanque, btnBombas, btnResumen });
            topPanel.Controls.Add(flow);

            rtbEstadisticas = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 10f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            tabEstadisticas.Controls.Add(rtbEstadisticas);
            tabEstadisticas.Controls.Add(topPanel);
        }

        private void MostrarCierreDiario()
        {
            var lista = _estadisticas.CierreDiario(dtpCierre.Value);
            rtbEstadisticas.Clear();
            RTB($"═══ CIERRE DIARIO: {dtpCierre.Value:dd/MM/yyyy} ═══\n\n", Color.Yellow);
            if (!lista.Any()) { RTB("  Sin abastecimientos este día.\n", Color.Gray); return; }
            foreach (var a in lista) RTB($"  {a}\n", Color.LightGreen);
            RTB($"\n  Total litros  : {_estadisticas.LitrosDiarios(dtpCierre.Value):F3} L\n", Color.Cyan);
            RTB($"  Total recaudado: Q{_estadisticas.TotalDiario(dtpCierre.Value):F2}\n", Color.Cyan);
        }

        private void MostrarInformePrepago()
        {
            var lista = _estadisticas.InformePrepago();
            rtbEstadisticas.Clear();
            RTB("═══ INFORME PREPAGOS ═══\n\n", Color.Yellow);
            if (!lista.Any()) { RTB("  Sin abastecimientos prepago.\n", Color.Gray); return; }
            foreach (var a in lista) RTB($"  {a}\n", Color.LightGreen);
            RTB($"\n  Total: {lista.Count} | Q{lista.Sum(a => a.MontoFinal):F2}\n", Color.Cyan);
        }

        private void MostrarInformeTanqueLleno()
        {
            var lista = _estadisticas.InformeTanqueLleno();
            rtbEstadisticas.Clear();
            RTB("═══ INFORME TANQUE LLENO ═══\n\n", Color.Yellow);
            if (!lista.Any()) { RTB("  Sin abastecimientos de tanque lleno.\n", Color.Gray); return; }
            foreach (var a in lista) RTB($"  {a}\n", Color.LightGreen);
            RTB($"\n  Total: {lista.Count} | Q{lista.Sum(a => a.MontoFinal):F2}\n", Color.Cyan);
        }

        private void MostrarInformeBombas()
        {
            rtbEstadisticas.Clear();
            RTB("═══ USO DE BOMBAS ═══\n\n", Color.Yellow);
            var conteo = _estadisticas.ConteoPorBomba();
            foreach (var kv in conteo.OrderByDescending(k => k.Value))
                RTB($"  Bomba {kv.Key}: {kv.Value} abastecimientos\n", Color.LightGreen);
            if (conteo.Values.Any(v => v > 0))
            {
                var (mas, menos) = _estadisticas.BombasMasYMenosUsadas();
                RTB($"\n  ★ Más usada:   Bomba {mas}\n", Color.Gold);
                RTB($"  ▼ Menos usada: Bomba {menos}\n", Color.OrangeRed);
            }
        }

        private void MostrarResumenGeneral()
        {
            var r = _estadisticas.ObtenerResumenGeneral();
            rtbEstadisticas.Clear();
            RTB("═══ RESUMEN GENERAL ═══\n\n", Color.Yellow);
            RTB($"  Total abastecimientos : {r.TotalAbastecimientos}\n", Color.LightGreen);
            RTB($"  Total litros servidos : {r.TotalLitros:F3} L\n", Color.LightGreen);
            RTB($"  Total recaudado       : Q{r.TotalRecaudado:F2}\n", Color.Cyan);
            RTB($"  Prepagos              : {r.TotalPrepago}\n", Color.LightBlue);
            RTB($"  Tanque lleno          : {r.TotalTanqueLleno}\n", Color.LightBlue);
            if (r.TotalAbastecimientos > 0)
            {
                RTB($"\n  ★ Bomba más usada  : Bomba {r.BombaMasUsada}\n", Color.Gold);
                RTB($"  ▼ Bomba menos usada: Bomba {r.BombaMenosUsada}\n", Color.OrangeRed);
            }
        }

        private void RTB(string texto, Color color)
        {
            rtbEstadisticas.SelectionStart = rtbEstadisticas.TextLength;
            rtbEstadisticas.SelectionLength = 0;
            rtbEstadisticas.SelectionColor = color;
            rtbEstadisticas.AppendText(texto);
        }

        // ─────────────────────────────────────────────
        // TAB: ARDUINO
        // ─────────────────────────────────────────────
        private void ConstruirTabConexion()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(10, 10, 10, 6) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill };

            cmbPuerto = new ComboBox
            {
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White
            };
            numBaud = new NumericUpDown
            {
                Width = 90,
                Minimum = 1200,
                Maximum = 115200,
                Value = 9600,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White
            };
            btnRefrescarPuertos = CrearBoton("🔄", Color.FromArgb(65, 65, 65));
            btnRefrescarPuertos.Width = 36;
            btnRefrescarPuertos.Click += (s, e) => RefrescarPuertos();

            btnConectar = CrearBoton("Conectar", Color.FromArgb(45, 120, 65));
            btnDesconectar = CrearBoton("Desconectar", Color.FromArgb(140, 45, 45));
            btnDesconectar.Enabled = false;
            btnConectar.Click += BtnConectar_Click;
            btnDesconectar.Click += (s, e) => { _arduino.Desconectar(); ActualizarEstadoConexion(); };

            lblEstadoConexion = new Label
            {
                Text = "● Desconectado",
                AutoSize = true,
                ForeColor = Color.OrangeRed,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(10, 8, 0, 0)
            };

            flow.Controls.AddRange(new Control[] { cmbPuerto, numBaud, btnRefrescarPuertos, btnConectar, btnDesconectar, lblEstadoConexion });
            topPanel.Controls.Add(flow);

            rtbLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 12, 12),
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            tabConexion.Controls.Add(rtbLog);
            tabConexion.Controls.Add(topPanel);
            RefrescarPuertos();
        }

        // ─────────────────────────────────────────────
        // LÓGICA DE NEGOCIO
        // ─────────────────────────────────────────────
        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCliente.Text))
            {
                MessageBox.Show("Ingresa el nombre del cliente.", "Campo requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int numBomba = cmbBomba.SelectedIndex + 1;
            Bomba bomba = _bombas[numBomba - 1];

            if (bomba.EnUso)
            {
                MessageBox.Show($"La Bomba {numBomba} ya está en uso.", "Bomba ocupada",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool esPrepago = cmbTipo.SelectedIndex == 0;
            double precio = _registro.ObtenerPrecioPorLitro();
            double monto = (double)numMonto.Value;

            // Para prepago: los litros se calculan del monto y el precio. No se pregunta nada.
            // Para tanque lleno: los litros los reporta el Arduino al terminar.
            double litros = esPrepago ? Math.Round(monto / precio, 3) : 0;

            var abast = new Abastecimiento
            {
                NumeroBomba = numBomba,
                NombreCliente = txtCliente.Text.Trim(),
                Tipo = esPrepago ? TipoPago.Prepago : TipoPago.TanqueLleno,
                MontoPagado = esPrepago ? monto : 0,
                LitrosSolicitados = litros,
                LitrosServidos = 0,
                MontoFinal = esPrepago ? monto : 0,
                PrecioPorLitro = precio,
                Completado = false
            };

            bomba.IniciarAbastecimiento(abast);
            _registro.AgregarAbastecimiento(abast);

            if (_arduino.EstaConectado)
                _arduino.EnviarOrden(numBomba, abast.Tipo, litros);
            else
                AgregarLog($"⚠ Arduino no conectado. Bomba {numBomba} iniciada en modo local.");

            ActualizarEstadoBombas();
            txtCliente.Clear();
            AgregarLog($"✓ Bomba {numBomba} iniciada — {abast.NombreCliente} | {abast.Tipo} | {litros:F3}L");
        }

        private void BtnDetener_Click(object sender, EventArgs e)
        {
            int numBomba = (int)((Button)sender).Tag;
            Bomba bomba = _bombas[numBomba - 1];
            if (!bomba.EnUso) return;

            if (_arduino.EstaConectado)
            {
                // Pedirle al Arduino que detenga; él responderá con los litros que alcanzó a servir
                _arduino.EnviarDetener(numBomba);
            }
            else
            {
                // Sin Arduino: calcular los litros según el tipo directamente
                var abast = bomba.AbastecimientoActual;
                double litrosFinales;

                if (abast.Tipo == TipoPago.Prepago)
                {
                    // Ya se sabía cuántos litros correspondían al monto; se marcan como servidos completos
                    litrosFinales = abast.LitrosSolicitados;
                }
                else
                {
                    // Tanque lleno sin Arduino: no hay sensor, se asume 0 (o el operador detiene por observación)
                    // En un escenario real esto nunca pasaría sin Arduino
                    litrosFinales = 0;
                }

                FinalizarAbastecimiento(numBomba, litrosFinales, true);
            }
        }

        /// <summary>
        /// Finaliza un abastecimiento con los litros reportados (por Arduino o calculados).
        /// No pregunta nada al usuario — todo se calcula.
        /// </summary>
        private void FinalizarAbastecimiento(int numBomba, double litrosServidos, bool completado)
        {
            Bomba bomba = _bombas[numBomba - 1];
            if (!bomba.EnUso) return;

            var abast = bomba.AbastecimientoActual;
            abast.LitrosServidos = litrosServidos;
            abast.Completado = completado;

            if (abast.Tipo == TipoPago.TanqueLleno)
            {
                // El cobro es exactamente lo que se sirvió
                abast.MontoFinal = Math.Round(litrosServidos * abast.PrecioPorLitro, 2);
            }
            else
            {
                // Prepago: si sirvió menos de lo solicitado, se cobra solo lo que se sirvió
                // Si sirvió todo, el monto final es el monto pagado original
                double cobrado = Math.Round(litrosServidos * abast.PrecioPorLitro, 2);
                abast.MontoFinal = Math.Min(cobrado, abast.MontoPagado);
            }

            _registro.ActualizarAbastecimiento(abast);
            bomba.FinalizarAbastecimiento();
            ActualizarEstadoBombas();
            AgregarLog($"✓ Bomba {numBomba} finalizada — {litrosServidos:F3}L | Q{abast.MontoFinal:F2}");
        }

        private void SuscribirEventosArduino()
        {
            _arduino.MensajeRecibido += (resp) =>
            {
                if (InvokeRequired) Invoke(new Action(() => ProcesarRespuestaArduino(resp)));
                else ProcesarRespuestaArduino(resp);
            };
            _arduino.LogRecibido += (msg) =>
            {
                if (InvokeRequired) Invoke(new Action(() => AgregarLog(msg)));
                else AgregarLog(msg);
            };
        }

        private void ProcesarRespuestaArduino(RespuestaArduino resp)
        {
            AgregarLog($"← Arduino: bomba {resp.bomba} → {resp.litrosServidos:F3}L completado={resp.completado}");
            // El Arduino reporta los litros exactos; el sistema calcula el monto solo
            FinalizarAbastecimiento(resp.bomba, resp.litrosServidos, resp.completado);
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            if (cmbPuerto.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un puerto COM.", "Sin puerto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _arduino.Conectar(cmbPuerto.SelectedItem.ToString(), (int)numBaud.Value);
            ActualizarEstadoConexion();
        }

        private void ActualizarEstadoConexion()
        {
            bool c = _arduino.EstaConectado;
            lblEstadoConexion.Text = c ? "● Conectado" : "● Desconectado";
            lblEstadoConexion.ForeColor = c ? Color.LimeGreen : Color.OrangeRed;
            btnConectar.Enabled = !c;
            btnDesconectar.Enabled = c;
        }

        private void RefrescarPuertos()
        {
            cmbPuerto.Items.Clear();
            var puertos = ArduinoService.ObtenerPuertosDisponibles();
            cmbPuerto.Items.AddRange(puertos);
            if (puertos.Length > 0) cmbPuerto.SelectedIndex = 0;
        }

        private void ActualizarEstadoBombas()
        {
            for (int i = 0; i < 4; i++)
            {
                bool enUso = _bombas[i].EnUso;
                lblEstadoBomba[i].Text = enUso
                    ? $"BOMBA {i + 1}\nEN USO\n{_bombas[i].AbastecimientoActual?.NombreCliente ?? ""}"
                    : $"BOMBA {i + 1}\nDISPONIBLE";
                lblEstadoBomba[i].ForeColor = enUso ? Color.OrangeRed : Color.LimeGreen;
                panelBombas[i].BackColor = enUso ? Color.FromArgb(60, 30, 30) : Color.FromArgb(30, 55, 30);
                btnDetener[i].Enabled = enUso;
            }
        }

        private void ActualizarPrecioLabel()
        {
            double p = _registro.ObtenerPrecioPorLitro();
            if (lblPrecioActual != null) lblPrecioActual.Text = $"Precio actual: Q{p:F2}/L";
            if (lblPrecio != null) lblPrecio.Text = $"Q{p:F2} / litro";
            ActualizarLitrosCalculados();
        }

        private void ActualizarLitrosCalculados()
        {
            if (lblLitrosCalc == null || cmbTipo == null || numMonto == null) return;
            if (cmbTipo.SelectedIndex == 0)
            {
                double litros = (double)numMonto.Value / _registro.ObtenerPrecioPorLitro();
                lblLitrosCalc.Text = $"{litros:F3} litros";
            }
            if (lblPrecio != null)
                lblPrecio.Text = $"Q{_registro.ObtenerPrecioPorLitro():F2} / litro";
        }

        private void AgregarLog(string msg)
        {
            rtbLog?.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            rtbLog?.ScrollToCaret();
        }

        private GroupBox CrearGroupBox(string titulo) => new GroupBox
        {
            Text = titulo,
            ForeColor = Color.Silver,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(38, 38, 38)
        };

        private Label CrearLabel(string texto) => new Label
        {
            Text = texto,
            Dock = DockStyle.Fill,
            ForeColor = Color.Silver,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9.5f)
        };

        private Button CrearBoton(string texto, Color color) => new Button
        {
            Text = texto,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            AutoSize = true,
            Padding = new Padding(6, 0, 6, 0),
            Cursor = Cursors.Hand
        };

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _arduino.Desconectar();
            base.OnFormClosing(e);
        }
    }
}