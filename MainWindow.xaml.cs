using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MatchGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Instanciamos el contador y un atributo para el tiempo
        TimeSpan time;
        DispatcherTimer timer;

        //Instanciamos otro temporizador para la partida en general
        TimeSpan tiempoPartida;
        DispatcherTimer temporizadorJuego;
        DispatcherTimer temporizador;
        DispatcherTimer temporizadorRonda;


        //Hace referencia a la primera imagen a la que se haga click de la sequencia,
        //Se llama si todavia no hay ninguna imagen girada
        Image firstClicked = null;

        //Hace referencia a la segunda imagen a la que se haga click de la sequencia
        //Se llama cuando ya hay una imagen girada
        Image secondClicked = null;


        // usaremos este random para obtener numeros aleatorios dentro de la lista
        Random random = new Random();

        //Aqui establecemos las Source para las imagenes, repetimos cada source 2 veces para que sean parejas
        List<string> imgs = new List<string>()
    {
            "/yellow.png","/yellow.png","/amarillo.png","/amarillo.png","/verde.png","/verde.png","/gris.png",
            "/gris.png","/marron.png","/marron.png","/blue.png","/blue.png","/lima.png","/lima.png",
            "/celeste.png","/celeste.png","/morado.png","/morado.png","/crewmate.png","/crewmate.png","/blancoynegro.png",
            "/blancoynegro.png","/tronja.png","/tronja.png","/hd.png","/hd.png","/UFO.png","/UFO.png","/button.png","/button.png"
    };

        public MainWindow()
        {
            InitializeComponent();
            AssignImagesToSquares();
            timer = new DispatcherTimer();


            //Establecemos el tiempo a 0 (para que no se inicie solo)
            time = TimeSpan.FromSeconds(0);

            //Definimos el contadorm con sus funciones
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                timer.Stop();

                if (time == TimeSpan.Zero) timer.Stop();
                time = time.Add(TimeSpan.FromSeconds(-1));

                //El contador cuando se ejecuta, oculta las imagenes y reinicia la sequencia
                if (firstClicked != null)
                {
                    firstClicked.Opacity = 0.0;
                    secondClicked.Opacity = 0.0;
                }

                firstClicked = null;
                secondClicked = null;
                temporizadorRonda.Stop();

            }, Application.Current.Dispatcher);

            tiempoPartida = TimeSpan.FromSeconds(121); //Un segundo mas porque es lo que tarda en cargar todo
            int timerINT = 120;
            temporizador = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, delegate
            {
                TimeTextBox.Text = "Tiempo: " + timerINT.ToString();
                timerINT--;
                if (timerINT < 0)
                {
                    temporizador.Stop();
                }
                else {
                    temporizador.Start();
                }

            }, Application.Current.Dispatcher);

            temporizadorJuego = new DispatcherTimer(tiempoPartida, DispatcherPriority.Normal, delegate
            {
                if (tiempoPartida == TimeSpan.Zero) timer.Stop();
                tiempoPartida = tiempoPartida.Add(TimeSpan.FromSeconds(-1));

                MessageBox.Show("Te has quedado sin tiempo, Perdiste");
                this.Close();

            },Application.Current.Dispatcher);

            temporizadorRonda = new DispatcherTimer(TimeSpan.FromSeconds(7), DispatcherPriority.Normal, delegate
            {
                if (secondClicked == null) {
                    firstClicked.Opacity = 0.0;
                    firstClicked = null;
                }

            }, Application.Current.Dispatcher);

            temporizador.Start();

        }




        /// <summary>
        /// Metodo que asigna una imagen aleatorio de la lista a cada cuadrado
        /// </summary>
        private void AssignImagesToSquares()
        {
            //Iteramos los elementos que contiene el grid
            //En nuestro XAML todos los elementos dentro de borders son Image
            //Asi que casteamos y establecemos una imagen de la lista
            //eliminadola para que no vuelva a salir
            foreach (UIElement control in paneles.Children)
            {
                Border border = control as Border;
                if (border != null)
                {
                    Image image = border.Child as Image;
                    if (image != null)
                    {
                        int randomNumber = random.Next(imgs.Count);
                        Uri uri = new Uri(imgs[randomNumber], UriKind.Relative);

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uri;
                        bitmap.EndInit();
                        image.Source = bitmap;
                        image.Opacity = 0.0;
                        image.Tag = bitmap.UriSource;
                        imgs.RemoveAt(randomNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Metodo que se lanza cuando el usuario hace click en una figura
        /// si esta oculta la muestra, en caso contrario no hace nada
        /// </summary>
        /// <param name="sender"> El objecto que han clickado </param>
        /// <param name="e"> Los argumentos del evento de clickar el objeto </param>
        private void image_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            // The timer is only on after two non-matching 
            // icons have been shown to the player, 
            // so ignore any clicks if the timer is running
            if (timer.IsEnabled)
            {
                return;
            }

            Image clickedImage = sender as Image;

            if (clickedImage != null)
            {
                //Si la imagen a la que hacemos click no es trasparente
                //Ignoramos el click
                if (clickedImage.Opacity == 1.0)
                    return;

                //Si la imagen a la que hacemos click es trasparente
                //La establecemos como el primer par de la pareja
                if (firstClicked == null)
                {
                    firstClicked = clickedImage;
                    firstClicked.Opacity = 1.0;

                    //Empezamos a contar desde que selecciona la primera imagen
                    temporizadorRonda.Start();

                    return;
                }

               

                //Si llegamos aqui, ya tenemos el primer par
                //Establecemos la imagen como el segundo par y la mostramos
                secondClicked = clickedImage;
                secondClicked.Opacity = 1.0;


                CheckForWinner();

                //Comprobamos si las dos imagenes son iguales para dejarlas giradas
                if (firstClicked.Tag.Equals(secondClicked.Tag))
                {
                    firstClicked = null;
                    secondClicked = null;
                    return;
                }

                //Si llegamos aqui, las dos imagenes no eran iguales
                //Empezamos la cuenta antras para volver a girar las 
                time = TimeSpan.FromSeconds(7.5);
                timer.Start();
            }

        }

        /// <summary>
        /// Verificamos cada imagen para ver si coinciden
        /// comparando su opacidad (0 para imagenes no giradas)
        /// Si todas los imagenes coinciden, el jugador gana
        /// </summary>
        private void CheckForWinner()
        {
            //Iteramos todas las imagenes para ver que no hay ningyna sin girar
            foreach (UIElement control in paneles.Children)
            {
                Border border = control as Border;
                if (border != null)
                {
                    Image image = border.Child as Image;
                    if (image != null)
                    {
                        if (image.Opacity == 0 || image.Opacity == 0.0)
                            return;
                    }
                }

                
            }
            //Si llegamos hasta aqui significa que hemos ganado, mostramos un mensaje de felicitacion
            temporizadorRonda.Stop();
            temporizadorJuego.Stop();
            MessageBox.Show("Has emparejado todas las imagenes!", "Felicidades");
            Close();


        }
    }
}
