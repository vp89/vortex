#!/bin/bash
dotnet build
./maelstrom/maelstrom test -w broadcast --bin bin/Debug/net6.0/vortex --time-limit 5 --log-stderr
