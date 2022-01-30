#!/bin/bash

# shellcheck disable=SC2068
command=$*
dotnet ef $command \
  -p=Data \
  -c=LocalDbContext \
  $([[ "$command" =~ 'migrations add' ]] && echo '-o=LocalMigrations')
