# Contributing

* Comments and identifiers are English only.
* Follow the existing file layout described in `docs/DEVELOPMENT.md`.
* Any new automation must be gated behind its own option and must not relax a
  vanilla requirement.
* Wrap reflective or patched calls in `SafeInvoke.Try`.
* Update `CHANGELOG.md`, `README.md` and all five translation tables in the
  same pull request.
