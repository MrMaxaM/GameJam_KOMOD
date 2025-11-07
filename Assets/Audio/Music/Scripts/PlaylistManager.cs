using System.Collections.Generic;
using UnityEngine;

// Позволяет выбирать, какую музыку (последовательность файлов) воспроизводить

public class PlaylistManager : MonoBehaviour
{
    [System.Serializable]
    public class Playlist
    {
        public string name;
        public AudioClip[] clips;
    }

    public List<Playlist> playlists = new List<Playlist>();

    private SequentialAudioPlayer player;
    private Playlist currentPlaylist;

    void Start()
    {
        player = GetComponent<SequentialAudioPlayer>();
        if (player == null)
        {
            Debug.LogError("SequentialAudioPlayer не найден на объекте.");
            return;
        }

        if (playlists.Count > 0)
        {
            currentPlaylist = playlists[0];
            player.PlayPlaylist(currentPlaylist.clips, false);
        }
    }

    public void PlayPlaylist(string playlistName, bool force = false)
    {
        var found = playlists.Find(p => p.name == playlistName);

        Debug.Log($"Плейлисты: {playlists}");
        if (found != null)
        {
            if (currentPlaylist != found)
            {
                Debug.Log($"Плейлист {found.name} найден");
                currentPlaylist = found;
                player.PlayPlaylist(found.clips, force);
            }
            else
                Debug.LogWarning($"Плейлист '{playlistName}'уже играет.");
        }
        else
        {
            Debug.LogWarning($"Плейлист '{playlistName}' не найден.");
        }
    }

    public void Stop()
    {
        player.StopPlaylist();
        currentPlaylist = null;
    }
}
