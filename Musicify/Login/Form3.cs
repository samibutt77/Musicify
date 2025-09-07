using Guna.UI2.WinForms;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace Login
{
    public partial class Form3 : Form
    {
        private WindowsMediaPlayer player;
        private List<string> paths = new List<string>();
        private int currentSongIndex = -1;
        private TrackBar songProgress;
        private string currentUserId;
        private Panel sidebar;
        private FlowLayoutPanel albumsPanel;
        private FlowLayoutPanel songsPanel;
        private FlowLayoutPanel playlistPanel;
        private FlowLayoutPanel songPanel;



        public Form3(string userid)
        {
           
            InitializeComponent();
            InitializeHomePage();
            currentUserId = userid;

            player = new WindowsMediaPlayer();
            player.controls.stop();
            player.PlayStateChange += Player_PlayStateChange;
            player.PositionChange += Player_PositionChange;

            // Form Properties
            this.Text = "Music Streaming App";
            this.BackColor = Color.Black;
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;

        }
        private void InitializeHomePage()
        {
            this.Controls.Clear();
            // Sidebar (Navigation)
            sidebar = new Panel
            {
                BackColor = Color.FromArgb(25, 25, 25),
                Dock = DockStyle.Left,
                Width = 250
            };

            Label appTitle = new Label
            {
                Text = "Musicify", // Custom App Name
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.Green,
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnHome = CreateSidebarButton("Home", "home_icon.png");
            Button btnSearch = CreateSidebarButton("Search", "search_icon.png");
            Button btnPlaylists = CreateSidebarButton("Playlists", "playlist_icon.png");
            Button likedSongsButton = CreateSidebarButton("liked Songs", "liked.png");

           
            likedSongsButton.FlatAppearance.BorderSize = 0;
            likedSongsButton.Click += LikedSongsButton_Click;

            //btnPlaylists.Click += playlist_Click;
            btnPlaylists.Click += new EventHandler(playlist_Click);



            sidebar.Controls.Add(btnPlaylists);
            sidebar.Controls.Add(btnSearch);
            sidebar.Controls.Add(likedSongsButton);
            sidebar.Controls.Add(btnHome);
            sidebar.Controls.Add(appTitle);

            // Main Content Area
            Panel mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                Padding = new Padding(10)
            };

            Label welcomeText = new Label
            {
                Text = "Welcome to Musicify!",
                Font = new Font("Arial", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            playlistPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                AutoScroll = true,
                Padding = new Padding(10),
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Add dummy playlist tiles
            for (int i = 1; i <= 10; i++)
            {
                Panel playlistTile = CreatePlaylistTile($"Playlist {i}", "album_icon.png");
                playlistPanel.Controls.Add(playlistTile);
            }

            mainContent.Controls.Add(playlistPanel);
            mainContent.Controls.Add(welcomeText);

            // Now Playing Bar
            Panel nowPlayingBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10)
            };

            PictureBox albumArtwork = new PictureBox
            {
                Image = Image.FromFile("default_album.png"), // Add a default album image
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Left
            };

            Label songInfo = new Label
            {
                Text = "Song Title - Artist Name",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                Padding = new Padding(10)
            };

            Panel playbackControls = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            FlowLayoutPanel controlLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.None, // Not docked, allows custom positioning
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(570, 1), // Adjust as needed to center buttons
                WrapContents = false // Ensure buttons are in a single row
            };

            Button btnPrevious = CreatePlaybackButton("prev_icon.png");
            Button btnPlayPause = CreatePlaybackButton("play_icon.png");
            Button btnNext = CreatePlaybackButton("next_icon.png");

            controlLayout.Controls.Add(btnPrevious);
            controlLayout.Controls.Add(btnPlayPause);
            controlLayout.Controls.Add(btnNext);

            playbackControls.Controls.Add(controlLayout);

            songProgress = new TrackBar
            {
                Dock = DockStyle.Bottom,
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            songProgress.ValueChanged += SongProgress_ValueChanged;


            nowPlayingBar.Controls.Add(songProgress);
            nowPlayingBar.Controls.Add(playbackControls);
            nowPlayingBar.Controls.Add(songInfo);
            nowPlayingBar.Controls.Add(albumArtwork);

            // Add Panels to Form
            this.Controls.Add(mainContent);
            this.Controls.Add(nowPlayingBar);
            this.Controls.Add(sidebar);


            // Button click handlers for Play, Pause, Previous, and Next
            btnPlayPause.Click += BtnPlayPause_Click;
            btnNext.Click += BtnNext_Click;
            btnPrevious.Click += BtnPrevious_Click;


            TextBox txtSearch = new TextBox
            {
                PlaceholderText = "Search Something...",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 12),
                Height = 40,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };


            sidebar.Controls.Add(txtSearch);
       
            // Attach an event handler for the sidebar search button
            btnSearch.Click += (s, e) =>
            {
                SearchSongs(txtSearch.Text, playlistPanel);
            };

            btnHome.Click += (s, e) => InitializeHomePage();// Reset to home page

        }
        //}

        private void InitializeplaylistPanel()
        {
            if (playlistPanel == null) // Check if the panel is already initialized
            {
                playlistPanel = new FlowLayoutPanel
                {
                    Name = "playlistPanel",
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.Black
                };
                this.Controls.Add(playlistPanel); // Add it to the form
            }

            playlistPanel.BringToFront(); // Bring the panel to the front
        }

        private void playlist_Click(object sender, EventArgs e)
        {
            ShowPlaylists();
        }

        
        private void ShowPlaylists()
        {
            InitializeplaylistPanel();

            // Clear previous content in the playlistPanel container
            playlistPanel.Controls.Clear();

            // Add the "Create Playlist" button at the top
            Button createPlaylistButton = new Button
            {
                Text = "Create Playlist",
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Size = new Size(200, 40),
                Margin = new Padding(10)
            };

            // Assign the event handler for the button
            createPlaylistButton.Click += (s, args) => CreatePlaylist(); // Assuming the function is called `CreatePlaylist`

            // Add the button to the playlist panel
            playlistPanel.Controls.Add(createPlaylistButton);

            // Connection string for the database
            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query to get playlists for the current user
                    string query = @"
SELECT PlaylistID, PlaylistName, NumberOfSongs, Duration
FROM Playlist
WHERE UserID = @UserID"; // Using UserID directly

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", currentUserId); // Assuming you have the current logged-in user's ID

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Create a panel for each playlist
                                Panel playlistsPanel = new Panel
                                {
                                    Size = new Size(300, 120),
                                    BackColor = Color.FromArgb(50, 50, 50),
                                    Margin = new Padding(10),
                                    BorderStyle = BorderStyle.FixedSingle
                                };

                                // Create a label to show the playlist name
                                Label playlistNameLabel = new Label
                                {
                                    Text = reader["PlaylistName"].ToString(),
                                    ForeColor = Color.White,
                                    Font = new Font("Arial", 12, FontStyle.Bold),
                                    Dock = DockStyle.Top
                                };

                                // Get the duration from the database (convert it to a string for display)
                                string duration = reader["Duration"].ToString();

                                // Create a label to show the number of songs and duration
                                Label playlistDetailsLabel = new Label
                                {
                                    Text = $"{reader["NumberOfSongs"]} Songs | Duration: {duration}",
                                    ForeColor = Color.White,
                                    Font = new Font("Arial", 10),
                                    Dock = DockStyle.Bottom
                                };

                                // Add the labels to the panel
                                playlistsPanel.Controls.Add(playlistNameLabel);
                                playlistsPanel.Controls.Add(playlistDetailsLabel);

                                // Store PlaylistID in the Tag property of the panel
                                playlistsPanel.Tag = reader["PlaylistID"];

                                // Add the playlist panel to the parent container (e.g., FlowLayoutPanel)
                                playlistPanel.Controls.Add(playlistsPanel);

                                // Handle click on the playlist panel
                                playlistsPanel.Click += (sender, e) =>
                                {
                                    // Retrieve PlaylistID from the Tag property
                                    int playlistID = (int)((Panel)sender).Tag;
                                    ShowSongsInPlaylist(playlistID); // Show songs in the selected playlist
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading playlists: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void InitializeSongPanel()
        {
            if (songPanel == null) // Check if the panel is already initialized
            {
                songPanel = new FlowLayoutPanel
                {
                    Name = "songPanel",
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.Black
                };
                this.Controls.Add(songPanel); // Add it to the form
            }

            songPanel.BringToFront(); // Bring the panel to the front
        }

        private void ShowSongsInPlaylist(int playlistID)
        {
            InitializeSongPanel();

            // Clear previous content
            songPanel.Controls.Clear();
            songPanel.Refresh(); // Force a refresh of the panel

            // Query to get songs from the selected playlist including the song path
            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
    SELECT Song.SongID, Song.Title, Song.Duration, Song.songPath
    FROM Song
    INNER JOIN AddSongtoPlaylist ON Song.SongID = AddSongtoPlaylist.SongID
    WHERE AddSongtoPlaylist.PlaylistID = @PlaylistID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PlaylistID", playlistID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Get the song path before reader is closed
                                string songPath = reader["songPath"] != DBNull.Value ? reader["songPath"].ToString() : string.Empty;

                                // Create a panel for each song
                                Panel songPanelItem = new Panel
                                {
                                    Size = new Size(300, 60),
                                    BackColor = Color.FromArgb(70, 70, 70),
                                    Margin = new Padding(5),
                                    BorderStyle = BorderStyle.FixedSingle
                                };

                                // Create a label for song name
                                Label songNameLabel = new Label
                                {
                                    Text = reader["Title"] != DBNull.Value ? reader["Title"].ToString() : "Unknown Song",
                                    ForeColor = Color.White,
                                    Font = new Font("Arial", 12),
                                    Dock = DockStyle.Top
                                };

                                // Create a label for song duration
                                string duration = reader["Duration"] != DBNull.Value ? reader["Duration"].ToString() : "Unknown Duration";
                                Label songDurationLabel = new Label
                                {
                                    Text = $"Duration: {duration}",
                                    ForeColor = Color.White,
                                    Font = new Font("Arial", 10),
                                    Dock = DockStyle.Bottom
                                };

                                // Create a button to play the song
                                Button playButton = new Button
                                {
                                    Text = "Play",
                                    ForeColor = Color.White,
                                    BackColor = Color.Green,
                                    Dock = DockStyle.Right,
                                    Width = 70
                                };

                                // Add play button functionality to use songPath
                                playButton.Click += (sender, e) =>
                                {
                                    if (!string.IsNullOrEmpty(songPath))
                                    {
                                        PlaySong(songPath); // Call PlaySong with songPath
                                    }
                                    else
                                    {
                                        MessageBox.Show("Song path not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                };

                                // Add the controls to the song panel
                                songPanelItem.Controls.Add(songNameLabel);
                                songPanelItem.Controls.Add(songDurationLabel);
                                songPanelItem.Controls.Add(playButton);

                                // Add the song panel to the parent container (e.g., FlowLayoutPanel)
                                songPanel.Controls.Add(songPanelItem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading songs: " + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }



        private void CreatePlaylist()
        {
            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            // Prompt the user for Playlist Name
            string playlistName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the name of the playlist:",
                "Create Playlist",
                "New Playlist");

            if (string.IsNullOrWhiteSpace(playlistName))
            {
                MessageBox.Show("Playlist name cannot be empty.");
                return;
            }

            // Insert the new playlist into the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Get the next PlaylistID
                    string getNextIDQuery = "SELECT ISNULL(MAX(PlaylistID), 0) + 1 FROM Playlist";
                    SqlCommand idCommand = new SqlCommand(getNextIDQuery, connection);

                    int nextPlaylistID = (int)idCommand.ExecuteScalar();

                    // Insert the new playlist with default values for NumberOfSongs and Duration
                    string insertPlaylistQuery = @"
                INSERT INTO Playlist (PlaylistID, PlaylistName, NumberOfSongs, Duration, UserID)
                VALUES (@PlaylistID, @PlaylistName, 0, '00:00:00', @UserID)";

                    SqlCommand insertCommand = new SqlCommand(insertPlaylistQuery, connection);
                    insertCommand.Parameters.AddWithValue("@PlaylistID", nextPlaylistID);
                    insertCommand.Parameters.AddWithValue("@PlaylistName", playlistName);
                    insertCommand.Parameters.AddWithValue("@UserID", currentUserId); // Use the current user's ID

                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Playlist created successfully!");
                        ShowPlaylists();
                    }
                    else
                    {
                        MessageBox.Show("Failed to create playlist. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }




        private void LikedSongsButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                MessageBox.Show("User is not logged in. Cannot load liked songs.");
                return;
            }

            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            FlowLayoutPanel likedSongsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Create a new panel for the main content area
            Panel mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                Padding = new Padding(10)
            };

            // Clear and re-add the controls: likedSongsPanel and sidebar
            this.Controls.Clear();
            this.Controls.Add(likedSongsPanel);  // Add the likedSongsPanel directly
            this.Controls.Add(sidebar);          // Add the sidebar back to the form

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT S.Title, S.Genre, S.Duration, S.ArtworkPath " +
                             "FROM Favorite F " +
                             "INNER JOIN Song S ON F.SongID = S.SongID " +
                             "WHERE F.UserID = @UserID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserId);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string genre = reader["Genre"].ToString();
                            string duration = reader["Duration"].ToString();
                            string artworkPath = reader["ArtworkPath"].ToString();

                            Panel songPanel = new Panel
                            {
                                Size = new Size(300, 150),
                                BackColor = Color.FromArgb(25, 25, 25),
                                Margin = new Padding(10),
                                BorderStyle = BorderStyle.FixedSingle,
                                Padding = new Padding(10)
                            };

                            PictureBox artworkImage = new PictureBox
                            {
                                Size = new Size(100, 100),
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                Image = File.Exists(artworkPath) ? Image.FromFile(artworkPath) : null,
                                Dock = DockStyle.Left
                            };

                            Label songLabel = new Label
                            {
                                Text = $"{title} ({genre})",
                                ForeColor = Color.White,
                                Font = new Font("Arial", 12, FontStyle.Bold),
                                Dock = DockStyle.Top,
                                Padding = new Padding(0, 0, 0, 5)
                            };

                            Label durationLabel = new Label
                            {
                                Text = $"Duration: {duration}",
                                ForeColor = Color.Gray,
                                Font = new Font("Arial", 10, FontStyle.Regular),
                                Dock = DockStyle.Top
                            };

                            songPanel.Controls.Add(durationLabel);
                            songPanel.Controls.Add(songLabel);
                            songPanel.Controls.Add(artworkImage);

                            likedSongsPanel.Controls.Add(songPanel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No liked songs found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading liked songs: {ex.Message}");
                }
            }
        }






        private Button CreateSidebarButton(string text, string iconPath)
        {
            Button button = new Button
            {
                Text = $"   {text}",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Regular),
                Dock = DockStyle.Top,
                Height = 60,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10, 0, 0, 0),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText
            };
            button.FlatAppearance.BorderSize = 0;

            if (System.IO.File.Exists(iconPath))
            {
                button.Image = Image.FromFile(iconPath);
                button.Image = new Bitmap(button.Image, new Size(24, 24)); // Scale icon size
            }
            else
            {
                button.Text = $"   {text} (No Icon)";
            }

            if (text == "Home")
            {
                button.Click += (s, e) => InitializeHomePage();  // Calls method to reset to home
            }

            return button;
        }

        // Helper: Create Playlist Tiles
        private Panel CreatePlaylistTile(string playlistName, string imagePath)
        {
            Panel tile = new Panel
            {
                Size = new Size(180, 220),
                BackColor = Color.FromArgb(25, 25, 25),
                Margin = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle // Add border for styling
            };

            PictureBox albumArt = new PictureBox
            {
                Image = System.IO.File.Exists(imagePath) ? Image.FromFile(imagePath) : null,
                Size = new Size(160, 160),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Top
            };

            Label playlistLabel = new Label
            {
                Text = playlistName,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom
            };

            tile.Controls.Add(playlistLabel);
            tile.Controls.Add(albumArt);

            return tile;
        }



  

        // Helper: Create Playback Buttons
        private Button CreatePlaybackButton(string iconPath)
        {
            return new Button
            {
                Size = new Size(50, 50),
                BackgroundImage = Image.FromFile(iconPath), // Playback control icons
                BackgroundImageLayout = ImageLayout.Stretch,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        // Play/Pause button handler
        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            if (player.playState == WMPPlayState.wmppsPlaying)
            {
                player.controls.pause();
            }
            else
            {
                if (currentSongIndex >= 0 && currentSongIndex < paths.Count)
                {
                    player.URL = paths[currentSongIndex];
                    player.controls.play();
                }
            }
        }

        // Next button handler
        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (currentSongIndex < paths.Count - 1)
            {
                currentSongIndex++;
                player.URL = paths[currentSongIndex];
                player.controls.play();
            }
        }

        // Previous button handler
        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (currentSongIndex > 0)
            {
                currentSongIndex--;
                player.URL = paths[currentSongIndex];
                player.controls.play();
            }
        }

        private void Player_PositionChange(double oldPosition, double newPosition)
        {
            // Update the TrackBar value as the song progresses
            if (player.currentMedia != null)
            {
                int progress = (int)((newPosition / player.currentMedia.duration) * 100);
                Invoke((MethodInvoker)(() => songProgress.Value = progress));
            }
        }

        // TrackBar value changed handler
        private void SongProgress_ValueChanged(object sender, EventArgs e)
        {
            // Check if the player is playing and if currentMedia is not null
            if (player.playState == WMPPlayState.wmppsPlaying && player.currentMedia != null)
            {
                // Sync the TrackBar with the song's playback position
                double newPosition = songProgress.Value * player.currentMedia.duration / 100;
                player.controls.currentPosition = newPosition;
            }
        }
        // Event handler for play state change (e.g., when the song is finished)
        private void Player_PlayStateChange(int newState)
        {
            if (newState == (int)WMPPlayState.wmppsMediaEnded)
            {
                if (currentSongIndex < paths.Count - 1)
                {
                    currentSongIndex++;
                    player.URL = paths[currentSongIndex];
                    player.controls.play();
                }
            }
        }


        private void InitializeAlbumsPanel()
        {
            // Initialize the panel only once
            albumsPanel = new FlowLayoutPanel
            {
                Name = "albumsPanel",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Black
            };
            this.Controls.Add(albumsPanel); // Add it to the form
        }


        private void SearchSongs(string query, FlowLayoutPanel resultsPanel)
        {
            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";
            resultsPanel.Controls.Clear(); // Clear previous results
            paths.Clear(); // Clear the paths list before adding new ones

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Query for songs
                    string songQuery = "SELECT SongID, Title, Genre, Duration, ArtworkPath, SongPath FROM Song WHERE Title LIKE @Query";
                    SqlCommand songCmd = new SqlCommand(songQuery, conn);
                    songCmd.Parameters.AddWithValue("@Query", "%" + query + "%");
                    SqlDataReader songReader = songCmd.ExecuteReader();

                    // Display song results
                    if (songReader.HasRows)
                    {
                        Label songHeader = new Label
                        {
                            Text = "Songs",
                            ForeColor = Color.White,
                            Font = new Font("Arial", 14, FontStyle.Bold),
                            Dock = DockStyle.Top,
                            Height = 30
                        };
                        resultsPanel.Controls.Add(songHeader);

                        while (songReader.Read())
                        {
                            int songID = Convert.ToInt32(songReader["SongID"]);
                            string songTitle = songReader["Title"].ToString();
                            string genre = songReader["Genre"].ToString();
                            string duration = songReader["Duration"].ToString();
                            string artworkPath = songReader["ArtworkPath"].ToString();
                            string songPath = songReader["SongPath"].ToString();

                            paths.Add(songPath);

                            // Create a panel for each song
                            Panel songPanel = new Panel
                            {
                                Size = new Size(300, 150),
                                BackColor = Color.FromArgb(25, 25, 25),
                                Margin = new Padding(10),
                                BorderStyle = BorderStyle.FixedSingle,
                                Padding = new Padding(10)
                            };

                            PictureBox artworkImage = new PictureBox
                            {
                                Size = new Size(100, 100),
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                Image = File.Exists(artworkPath) ? Image.FromFile(artworkPath) : null,
                                Dock = DockStyle.Left
                            };

                            Label songLabel = new Label
                            {
                                Text = $"{songTitle} ({genre})",
                                ForeColor = Color.White,
                                Font = new Font("Arial", 12, FontStyle.Bold),
                                Dock = DockStyle.Top,
                                Padding = new Padding(0, 0, 0, 5)
                            };

                            Label durationLabel = new Label
                            {
                                Text = $"Duration: {duration}",
                                ForeColor = Color.Gray,
                                Font = new Font("Arial", 10, FontStyle.Regular),
                                Dock = DockStyle.Top
                            };

                            Button playButton = new Button
                            {
                                Text = "Play",
                                ForeColor = Color.White,
                                BackColor = Color.Green,
                                FlatStyle = FlatStyle.Flat,
                                Dock = DockStyle.Bottom,
                                Height = 30
                            };
                            playButton.Click += (sender, e) => PlaySong(songPath);

                            Button addToPlaylistButton = new Button
                            {
                                Text = "Add to Playlist",
                                ForeColor = Color.White,
                                BackColor = Color.DarkOrange,
                                FlatStyle = FlatStyle.Flat,
                                Dock = DockStyle.Bottom,
                                Height = 30
                            };

                            // Event for adding the song to a playlist
                            addToPlaylistButton.Click += (sender, e) => AddToPlaylist(songID);

                            songPanel.Controls.Add(addToPlaylistButton);

                            songPanel.Controls.Add(playButton);
                            songPanel.Controls.Add(durationLabel);
                            songPanel.Controls.Add(songLabel);
                            songPanel.Controls.Add(artworkImage);

                            resultsPanel.Controls.Add(songPanel);
                        }
                    }
                    songReader.Close();

                    // Query for artists
                    string artistQuery = "SELECT ArtistName, Followers, Genre, Country, Description, ImagePath FROM Artist WHERE ArtistName LIKE @Query";
                    SqlCommand artistCmd = new SqlCommand(artistQuery, conn);
                    artistCmd.Parameters.AddWithValue("@Query", "%" + query + "%");
                    SqlDataReader artistReader = artistCmd.ExecuteReader();

                    // Display artist results
                    if (artistReader.HasRows)
                    {
                        Label artistHeader = new Label
                        {
                            Text = "Artists",
                            ForeColor = Color.White,
                            Font = new Font("Arial", 14, FontStyle.Bold),
                            Dock = DockStyle.Top,
                            Height = 30
                        };
                        resultsPanel.Controls.Add(artistHeader);

                        while (artistReader.Read())
                        {
                            string artistName = artistReader["ArtistName"].ToString();
                            string followers = artistReader["Followers"].ToString();
                            string genre = artistReader["Genre"].ToString();
                            string country = artistReader["Country"].ToString();
                            string description = artistReader["Description"].ToString();
                            string imagePath = artistReader["ImagePath"].ToString();

                            Panel artistPanel = new Panel
                            {
                                Size = new Size(600, 350),
                                BackColor = Color.FromArgb(25, 25, 25),
                                Margin = new Padding(10),
                                BorderStyle = BorderStyle.FixedSingle,
                                Padding = new Padding(10)
                            };

                            PictureBox artistImage = new PictureBox
                            {
                                Size = new Size(115, 70),
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                Image = File.Exists(imagePath) ? Image.FromFile(imagePath) : null,
                                Dock = DockStyle.Left,
                                Margin = new Padding(0, 0, 10, 0)
                            };

                            Label artistLabel = new Label
                            {
                                Text = $"{artistName}\nGenre: {genre}\nCountry: {country}\nFollowers: {followers}\n\nDescription: {description}",
                                ForeColor = Color.White,
                                Font = new Font("Arial", 12, FontStyle.Bold),
                                Dock = DockStyle.Fill,
                                Padding = new Padding(5)
                            };

                            Button viewDetailsButton = new Button
                            {
                                Text = "View Albums",
                                ForeColor = Color.White,
                                BackColor = Color.Blue,
                                FlatStyle = FlatStyle.Flat,
                                Dock = DockStyle.Bottom,
                                Height = 30
                            };

                            viewDetailsButton.Click += (sender, e) =>
                            {
                                Debug.WriteLine($"Button clicked for {artistName}");
                                LoadAlbums(artistName);
                            };

                            artistPanel.Controls.Add(viewDetailsButton);
                            artistPanel.Controls.Add(artistLabel);
                            artistPanel.Controls.Add(artistImage);

                            resultsPanel.Controls.Add(artistPanel);
                        }
                    }
                    artistReader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching for songs or artists: {ex.Message}");
                }
            }
        }

        private void AddToPlaylist(int songID)
        {
            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Fetch playlists for the current user
                string query = "SELECT PlaylistID, PlaylistName FROM Playlist WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserId);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable playlistsTable = new DataTable();
                adapter.Fill(playlistsTable);

                // Create a form to let the user choose a playlist
                using (Form selectPlaylistForm = new Form())
                {
                    selectPlaylistForm.Text = "Select Playlist";
                    selectPlaylistForm.Size = new Size(300, 200);

                    ComboBox playlistDropdown = new ComboBox
                    {
                        Dock = DockStyle.Top,
                        DataSource = playlistsTable,
                        DisplayMember = "PlaylistName",
                        ValueMember = "PlaylistID"
                    };

                    Button addButton = new Button
                    {
                        Text = "Add",
                        Dock = DockStyle.Bottom,
                        Height = 40
                    };

                    addButton.Click += (sender, e) =>
                    {
                        try
                        {
                            // Insert the song into the selected playlist
                            int selectedPlaylistID = (int)playlistDropdown.SelectedValue;

                            // Fetch song duration
                            string durationQuery = "SELECT Duration FROM Song WHERE SongID = @SongID";
                            SqlCommand durationCmd = new SqlCommand(durationQuery, conn);
                            durationCmd.Parameters.AddWithValue("@SongID", songID);
                            string songDuration = durationCmd.ExecuteScalar()?.ToString() ?? "00:00";

                            // Insert into AddSongtoPlaylist
                            string dateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string insertQuery = "INSERT INTO AddSongtoPlaylist (SongID, PlaylistID, DateAdded) VALUES (@SongID, @PlaylistID, @DateAdded)";
                            SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                            insertCmd.Parameters.AddWithValue("@SongID", songID);
                            insertCmd.Parameters.AddWithValue("@PlaylistID", selectedPlaylistID);
                            insertCmd.Parameters.AddWithValue("@DateAdded", dateAdded);
                            insertCmd.ExecuteNonQuery();

                            // Update the playlist's NumberOfSongs and Duration
                            string updateQuery = @"
                        UPDATE Playlist
                        SET NumberOfSongs = NumberOfSongs + 1,
                            Duration = DATEADD(SECOND, DATEDIFF(SECOND, 0, @SongDuration), Duration)
                        WHERE PlaylistID = @PlaylistID";
                            SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                            updateCmd.Parameters.AddWithValue("@SongDuration", TimeSpan.Parse(songDuration));
                            updateCmd.Parameters.AddWithValue("@PlaylistID", selectedPlaylistID);
                            updateCmd.ExecuteNonQuery();

                            MessageBox.Show("Song added to playlist successfully!");
                            selectPlaylistForm.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error adding song to playlist: {ex.Message}");
                        }
                    };

                    selectPlaylistForm.Controls.Add(playlistDropdown);
                    selectPlaylistForm.Controls.Add(addButton);

                    selectPlaylistForm.ShowDialog();
                }
            }
        }




        private void InitializeSongsPanel()
        {
            // Initialize the panel only once
            songsPanel = new FlowLayoutPanel
            {
                Name = "songsPanel",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Black
            };
            this.Controls.Add(songsPanel); // Add it to the form
        }

        private void LoadAlbums(string artistName)
        {
            InitializeAlbumsPanel();
            InitializeSongsPanel();
            albumsPanel.Controls.Clear();

            Debug.WriteLine($"Loading albums for artist: {artistName}");

            string connectionString = "Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT 
                                a.Title AS AlbumTitle,
                                a.ReleaseDate,
                                a.NumberOfSongs,
                                a.Duration AS AlbumDuration,
                                a.ImagePath,
                                a.AlbumID,
                                s.Title AS SongTitle,
                                s.Duration AS SongDuration,
                                s.SongPath
                             FROM Album a
                             LEFT JOIN Song s ON a.AlbumID = s.AlbumID
                             WHERE a.ArtistID = (SELECT ArtistID FROM Artist WHERE ArtistName = @ArtistName)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ArtistName", artistName);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            Dictionary<int, (string albumTitle, string releaseDate, string numberOfSongs, string duration, string imagePath, List<(string songTitle, string songDuration, string songPath)> songs)> albumData = new();

                            while (reader.Read())
                            {
                                int albumID = reader.GetInt32(reader.GetOrdinal("AlbumID"));
                                string albumTitle = reader["AlbumTitle"].ToString();
                                string releaseDate = Convert.ToDateTime(reader["ReleaseDate"]).ToString("yyyy-MM-dd");
                                string numberOfSongs = reader["NumberOfSongs"].ToString();
                                string duration = reader["AlbumDuration"].ToString();
                                string imagePath = reader["ImagePath"].ToString();

                                string songTitle = reader["SongTitle"] as string;
                                string songDuration = reader["SongDuration"] as string;
                                string songPath = reader["SongPath"] as string;

                                if (!albumData.ContainsKey(albumID))
                                {
                                    albumData[albumID] = (albumTitle, releaseDate, numberOfSongs, duration, imagePath, new List<(string, string, string)>());
                                }

                                if (!string.IsNullOrEmpty(songTitle))
                                {
                                    albumData[albumID].songs.Add((songTitle, songDuration, songPath));
                                }
                            }

                            foreach (var album in albumData.Values)
                            {
                                // Create a panel for each album
                                Panel albumPanel = new Panel
                                {
                                    Size = new Size(850, 350),
                                    BackColor = Color.FromArgb(30, 30, 30),
                                    Margin = new Padding(10),
                                    BorderStyle = BorderStyle.FixedSingle,
                                    Padding = new Padding(10)
                                };

                                // Add album image
                                PictureBox albumImage = new PictureBox
                                {
                                    Size = new Size(100, 100),
                                    SizeMode = PictureBoxSizeMode.StretchImage,
                                    Image = File.Exists(album.imagePath) ? Image.FromFile(album.imagePath) : null,
                                    Dock = DockStyle.Left,
                                    Margin = new Padding(0, 0, 10, 0)
                                };

                                // Add album details
                                Label albumDetails = new Label
                                {
                                    Text = $"Title: {album.albumTitle}\nRelease Date: {album.releaseDate}\nSongs: {album.numberOfSongs}\nDuration: {album.duration}",
                                    ForeColor = Color.White,
                                    Font = new Font("Arial", 10),
                                    Dock = DockStyle.Fill
                                };

                                // Add a button to load songs
                                Button loadSongsButton = new Button
                                {
                                    Text = "View Songs",
                                    ForeColor = Color.White,
                                    BackColor = Color.Blue,
                                    FlatStyle = FlatStyle.Flat,
                                    Dock = DockStyle.Bottom,
                                    Height = 30
                                };

                                loadSongsButton.Click += (sender, e) =>
                                {
                                    // Clear songsPanel and load songs for the selected album
                                    songsPanel.Controls.Clear();

                                    if (album.songs.Count > 0)
                                    {
                                        foreach (var (songTitle, songDuration, songPath) in album.songs)
                                        {
                                            Panel songPanel = new Panel
                                            {
                                                Size = new Size(800, 40),
                                                BackColor = Color.FromArgb(50, 50, 50),
                                                Margin = new Padding(5),
                                                BorderStyle = BorderStyle.None
                                            };

                                            // Song title label
                                            Label songTitleLabel = new Label
                                            {
                                                Text = $"Title: {songTitle}",
                                                ForeColor = Color.White,
                                                Font = new Font("Arial", 10),
                                                AutoSize = true,
                                                Location = new Point(10, 5)
                                            };

                                            // Song duration label
                                            Label songDurationLabel = new Label
                                            {
                                                Text = $"Duration: {songDuration}",
                                                ForeColor = Color.White,
                                                Font = new Font("Arial", 10),
                                                AutoSize = true,
                                                Location = new Point(10, 25)
                                            };

                                            Button playButton = new Button
                                            {
                                                Text = "Play",
                                                ForeColor = Color.White,
                                                BackColor = Color.Green,
                                                FlatStyle = FlatStyle.Flat,
                                                Size = new Size(50, 30),
                                                Location = new Point(songPanel.Width - 60, 5)
                                            };

                                            playButton.Click += (s, args) => PlaySong(songPath);

                                            songPanel.Controls.Add(songTitleLabel);
                                            songPanel.Controls.Add(songDurationLabel);
                                            songPanel.Controls.Add(playButton);
                                            songsPanel.Controls.Add(songPanel);
                                        }
                                    }
                                    else
                                    {
                                        songsPanel.Controls.Add(new Label
                                        {
                                            Text = "No songs found in this album.",
                                            ForeColor = Color.Gray,
                                            Font = new Font("Arial", 10, FontStyle.Italic),
                                            AutoSize = true
                                        });
                                    }

                                    songsPanel.BringToFront();
                                };

                                albumPanel.Controls.Add(loadSongsButton);
                                albumPanel.Controls.Add(albumDetails);
                                albumPanel.Controls.Add(albumImage);

                                // Add the album panel to the main albums panel
                                albumsPanel.Controls.Add(albumPanel);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading albums: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            albumsPanel.BringToFront();
        }















        private void HeartButton_Click(object sender, EventArgs e)
        {
            Button heartButton = sender as Button;
            int songID = (int)heartButton.Tag;

            using (SqlConnection connection = new SqlConnection("Data Source=.;Initial Catalog=MyDatabase;Integrated Security=True;"))
            {
                connection.Open();
                string query = "INSERT INTO Favorite (FavoriteID, AddedDate, UserID, SongID) VALUES (@FavoriteID, @AddedDate, @UserID, @SongID)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    Guid favoriteID = Guid.NewGuid();

                    command.Parameters.AddWithValue("@FavoriteID", favoriteID);
                    command.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                    command.Parameters.AddWithValue("@UserID", currentUserId); // Replace with actual logged-in UserID
                    command.Parameters.AddWithValue("@SongID", songID);

                    try
                    {
                        command.ExecuteNonQuery();

                        // Change heart color to red to indicate it's added to favorites
                        heartButton.ForeColor = Color.DarkBlue;
                        heartButton.BackColor = Color.Transparent;

                        // Show a smooth toast notification
                        ShowToast("Added to Favorites!");

                        heartButton.Enabled = false;

                    }
                    catch (Exception ex)
                    {
                        ShowToast($"Error: {ex.Message}", isError: true);
                    }
                }
            }
        }

        private void ShowToast(string message, bool isError = false)
        {
            Label toast = new Label
            {
                Text = message,
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = isError ? Color.DarkRed : Color.Green,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Padding = new Padding(10),
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(toast);
            this.PerformLayout(); // Force layout to get the correct size

            // Position at the bottom-center of the form
            toast.Location = new Point((this.ClientSize.Width - toast.Width) / 2, this.ClientSize.Height - toast.Height - 10);
            toast.BringToFront();

            // Animate fade-out
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 3000 }; // Show for 3 seconds
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                this.Controls.Remove(toast);
                toast.Dispose();
            };
            timer.Start();
        }




        private void PlaySong(string songPath)
        {
          
             player.URL = songPath;
            player.controls.play();
        }


        private void Form3_Load(object sender, EventArgs e)
        {

        }

    }
}





