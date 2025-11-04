using System.Collections.Generic;
using UnityEngine;

// Позволяет выбирать, какую музыку (последовательность файлов) воспроизводить
// Если верить gpt, можно вызывать в любом скрипте так:
// FindObjectOfType<AudioSequenceManager>().PlayPlaylist("ТутНазваниеПоследовательности");

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
            player.PlayPlaylist(currentPlaylist.clips);
        }
    }

    public void PlayPlaylist(string playlistName)
    {
        var found = playlists.Find(p => p.name == playlistName);
        if (found != null)
        {
            currentPlaylist = found;
            player.PlayPlaylist(found.clips);
        }
        else
        {
            Debug.LogWarning($"Плейлист '{playlistName}' не найден.");
        }
    }

    public void Stop()
    {
        player.StopPlaylist();
    }
}
