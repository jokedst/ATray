namespace RepositoryManager
{
    /// <summary>
    /// Current (or last known) status on a repository
    /// </summary>
    public enum RepoStatus
    {
        Unknown,
        /// <summary> No origin set </summary>
        Disconnected,
        /// <summary> No local changes, up to date with remote </summary>
        Clean,
        /// <summary> Local changes only </summary>
        Dirty,
        /// <summary> No local changes, behind remote </summary>
        Behind,
        /// <summary> Local changes and remote updates. Can not merge cleanly or haven't tried </summary>
        Conflict,
        /// <summary> Local changes and remote updates. Can merge cleanly </summary>
        Mergeable,
        /// <summary> Could not retrieve status </summary>
        Error,
        /// <summary> Local commits are not pushed </summary>
        Ahead
    }
}
/*
LOCAL STATE		ORIGIN STATE	CONFL	COM.CFL	STATE			AVAILABLE ACTIONS
(any)			(no origin)	    -				Disconnected	configure remote
clean			unchanged		-				Clean   		(none)
clean			changed			-				Behind			pull
local edits		unchanged		-				Dirty   		commit, push
local edits		changed			no				fforwardable	stull
local edits		changed			yes				dirty conflict	com+reb, stull+resolve, com+merge
local commits	unchanged		-				Ahead   		push, reset
local commits	changed			no				Mergable		merge, rebase
local commits	changed			yes				Conflict		merge or rebase + resolve
local com+dirty	unchanged		-				dirty Ahead 	commit, push, revert, reset
local com+dirty	changed			no				dirty mergable	mergepush, rebasepush
local com+dirty	changed			yes		no		mergable WS-cfl	stullmerge, commitresolve, revertmerge
local com+dirty	changed			yes		yes		conflict+		commitresolve, stullresolve

*/
