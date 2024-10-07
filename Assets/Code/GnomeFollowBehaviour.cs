namespace Gnome
{
    public class GnomeFollowBehaviour : GnomeAgent.IBehaviour
    {
        private readonly GnomeAgent gnome;
        private readonly Crowd crowd;

        public int Priority => crowd.Priority;

        public GnomeFollowBehaviour(
            GnomeAgent gnome,
            Crowd crowd)
        {
            this.gnome = gnome;
            this.crowd = crowd;
        }

        public void Start()
        {
            crowd.Invite(gnome);
        }

        public void End()
        {
            crowd.Exile(gnome);
        }
    }
}