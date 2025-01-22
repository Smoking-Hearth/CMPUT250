# Commit Message Reference (for convenience)
```
type(scope; issue): msg
```

Where `type` is one of:
- feat
- fix
- refactor
- perf
- create
- update
- delete
- docs
- chore

`scope` indicates the part of the game. Different scopes can be seen in the 
`S-...` tags attached to issues.

`issue` is the number of the issue the commit is related to if any. e.g. `#2`.
This basically just tells Github to show that commit under that issue when people
are browsing it. 
> NOTE: If closing an issue add "close" before the number and Github will automatically
> close it.

`msg` is active present tense describing the commit. Consider: "allowed players to walk through walls".
We save a couple character in present tense: "allow players to walk through walls".

(Conventional Commits)[https://www.conventionalcommits.org/en/v1.0.0/]

# A note on LFS 
Binary files are annoying to work with. So we're using
[git-lfs](https://git-lfs.com/) to store our asset files in a separate location
from the main repository. This isn't really necessary due to VCS diffing
causing us to store a basically new file each time we update the assets, since
git is a [bit
weird](https://git-scm.com/book/en/v2/Getting-Started-What-is-Git%3F) compared
to other VCS. 

This is mainly here because we've got limits on the maximum size of files
Github allows you to stick into a repo. If we hit that limit we'll stick the files
into the lfs storage space to workaround&mdash;but I don't think we'll hit that limit.

Note that you need git-lfs installed for the above to work. If you have Github
Desktop you will have it installed automatically. If you're using classic `git`
you'll probably want to read/sneak a peak at: 

 - [About Git Large File Storage](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-git-large-file-storage)
 - [The man pages](https://github.com/git-lfs/git-lfs/tree/main/docs/man)

As a good practice, to avoid going crazy because you can't use play a sound
because the file associated to it contains a URL, get in the habit of prefixing git
subcommands with lfs. e.g.

```
git lfs clone
```

