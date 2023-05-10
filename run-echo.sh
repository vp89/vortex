#!/bin/bash
dotnet build
./maelstrom/maelstrom test -w echo --bin bin/Debug/net6.0/vortex --node-count 1 --time-limit 3 --log-stderr
