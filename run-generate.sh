#!/bin/bash
dotnet build
./maelstrom/maelstrom test -w unique-ids --bin bin/Debug/net6.0/vortex --node-count 3 --time-limit 30 --availability total --nemesis partition --log-stderr
