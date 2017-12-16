namespace RepositoryManager
{
    using System;

    /// <summary>
    /// Current (or last known) status on a repository
    /// </summary>
    [Flags]
    public enum RepoStatus
    {
        /// <summary> Update has never run  </summary>
        Unknown = 0,

        /// <summary> No origin set </summary>
        Disconnected = 1,

        /// <summary> Could not retrieve status </summary>
        Error = 2,

        /// <summary> No local changes, up to date with remote </summary>
        Clean = 4,

        /// <summary> Local changes and remote updates. Can merge cleanly </summary>
        Mergeable = 8,

        /// <summary> Local changes only </summary>
        Dirty = Clean & LocalChanges,

        /// <summary> No local changes, behind remote </summary>
        Behind = Clean & RemoteUnmergedCommits,

        /// <summary> Local changes and remote updates. Can not merge cleanly or haven't tried </summary>
        ChangesConflict = Clean & RemoteUnmergedCommits & LocalChanges,

        /// <summary> Local commits and remote updates. Can not merge cleanly or haven't tried </summary>
        Conflict = Clean & RemoteUnmergedCommits & LocalUnpushedCommits,

        /// <summary> Local commits + changes and remote updates. Can not merge cleanly or haven't tried </summary>
        ConflictAndModified = Clean & RemoteUnmergedCommits & LocalUnpushedCommits & LocalChanges,

        /// <summary> Local commits are not pushed </summary>
        Ahead = Clean & LocalUnpushedCommits,

        /// <summary> Uncommitted changes and lLocal commits are not pushed </summary>
        DirtyAhead = Clean & LocalChanges & LocalUnpushedCommits,

        /// <summary> (flag) has local uncommited changes </summary>
        LocalChanges = 16,

        /// <summary> (flag) has local commited changes </summary>
        LocalUnpushedCommits = 32,

        /// <summary> (flag) has remote unmerged changes </summary>
        RemoteUnmergedCommits = 64,
    }
}
/*
LOCAL STATE		ORIGIN STATE	CONFL	COM.CFL	STATE			AVAILABLE ACTIONS
(any)			(no origin)	    -				Disconnected	configure remote
clean			unchanged		-				Clean   		(fetch?)
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
