# Branch Migration: main → master

**Migration Date**: 2025-11-04
**Repository**: DynamicUserDifficulty
**GitHub**: https://github.com/The1Studio/DynamicUserDifficulty

---

## Overview

This document records the migration of the DynamicUserDifficulty repository's default branch from `main` to `master`.

## Migration Details

### Timeline

- **Branch Created**: November 4, 2025
- **GitHub Default Changed**: November 4, 2025
- **Status**: ✅ Complete

### Actions Taken

1. **Created `master` branch** from `main`
   ```bash
   git checkout -b master
   git push -u origin master
   ```

2. **Updated GitHub default branch**
   - Repository Settings → Branches → Default branch
   - Changed from `main` to `master`
   - Confirmed the change

3. **Verified branch status**
   - Both `main` and `master` branches exist
   - `master` is now the default branch
   - All commits are synchronized

### Current Branch State

```bash
# Active branches
* master          # ← Default branch (current)
  main            # Legacy branch (kept for backward compatibility)

# Remote tracking
origin/master    # ← Default remote branch
origin/main      # Legacy remote branch
origin/HEAD      # → Points to origin/main (will update on next fetch)
```

## Impact on Development

### For Active Development

- **New clones** will default to `master` branch
- **Existing clones** need to update their tracking:
  ```bash
  git fetch origin
  git checkout master
  git branch --set-upstream-to=origin/master master
  ```

### For Submodule Integration

The parent repository (UnityFoodieSizzle) tracks this as a submodule:

**Submodule Configuration**:
```gitmodules
[submodule "Packages/DynamicUserDifficulty"]
    path = Packages/DynamicUserDifficulty
    url = git@github.com:The1Studio/DynamicUserDifficulty.git
    branch = master  # Updated to track master
```

**Update Submodule Reference**:
```bash
# In parent repository
cd /mnt/Work/1M/FoodieSizzle/UnityFoodieSizzle
git submodule update --remote Packages/DynamicUserDifficulty
```

## Backward Compatibility

### Legacy `main` Branch

- The `main` branch **remains available** for backward compatibility
- Existing integrations using `main` will continue to work
- **Recommendation**: Update to `master` for new projects

### Migration Path for Existing Projects

If you have an existing project using this package:

**Option 1: Update to master (Recommended)**
```bash
cd Packages/DynamicUserDifficulty
git fetch origin
git checkout master
git pull origin master
```

**Option 2: Continue using main**
```bash
# No action needed
# The main branch will continue to receive updates for now
```

## GitHub Actions & CI/CD

### Workflow Updates

If you have GitHub Actions workflows, update branch triggers:

```yaml
# Before
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

# After
on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]
```

### Status Badges

Update repository badges in README.md to reflect the new default branch:

```markdown
[![Tests](https://github.com/The1Studio/DynamicUserDifficulty/actions/workflows/tests.yml/badge.svg?branch=master)](https://github.com/The1Studio/DynamicUserDifficulty/actions/workflows/tests.yml)
```

## Unity Package Manager

### UPM Integration

**Git URL** (unchanged):
```
https://github.com/The1Studio/DynamicUserDifficulty.git
```

**With Branch Specification**:
```json
{
  "dependencies": {
    "com.theone.dynamicuserdifficulty": "https://github.com/The1Studio/DynamicUserDifficulty.git#master"
  }
}
```

## Documentation Updates

### Files Updated

The following documentation files have been updated to reflect the branch migration:

- ✅ `Documentation/BRANCH_MIGRATION.md` (this file) - Created
- ✅ `Documentation/INDEX.md` - Added migration reference
- ✅ `README.md` - Added migration note
- ✅ `CHANGELOG.md` - Recorded migration in changelog

### References to Update

When referencing the repository in documentation:

- ✅ Default branch is now `master`
- ✅ GitHub URLs point to `master` branch
- ✅ Clone instructions reference `master`

## Rationale

### Why Migrate?

The migration from `main` to `master` was performed for:

1. **Consistency**: Align with TheOne Studio's standard branch naming conventions
2. **Integration**: Match the naming scheme of parent repositories
3. **Compatibility**: Ensure seamless submodule integration with existing projects

### Industry Context

While many projects have moved from `master` to `main`, this migration follows the opposite direction to maintain consistency within TheOne Studio's ecosystem.

## Troubleshooting

### Issue: Local branch still points to main

**Solution**:
```bash
git checkout master
git branch --set-upstream-to=origin/master master
```

### Issue: Submodule not updating

**Solution**:
```bash
# In parent repository
git submodule sync
git submodule update --remote --force Packages/DynamicUserDifficulty
```

### Issue: GitHub Actions failing

**Solution**: Update workflow files to reference `master` instead of `main`

### Issue: Clone still defaults to main

**Solution**:
- Wait for GitHub's cache to clear (up to 24 hours)
- Or explicitly specify branch: `git clone -b master <url>`

## Verification

### Verify Branch Status

```bash
# Check current branch
git branch -a

# Expected output:
#   main
# * master
#   remotes/origin/HEAD -> origin/main  # Will update
#   remotes/origin/main
#   remotes/origin/master
```

### Verify GitHub Default

Visit: https://github.com/The1Studio/DynamicUserDifficulty

- Default branch badge should show: `master`
- Repository dropdown should default to `master`
- Clone instructions should reference `master`

## Additional Resources

- **Repository**: https://github.com/The1Studio/DynamicUserDifficulty
- **Package Documentation**: [README.md](../README.md)
- **Integration Guide**: [IntegrationGuide.md](IntegrationGuide.md)
- **GitHub Docs**: [Managing branches in your repository](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-branches-in-your-repository)

## Support

For questions or issues related to this migration:

1. Check this document first
2. Review the troubleshooting section
3. Contact TheOne Studio development team
4. Open an issue on GitHub: https://github.com/The1Studio/DynamicUserDifficulty/issues

---

**Document Version**: 1.0.0
**Last Updated**: 2025-11-04
**Status**: ✅ Migration Complete
