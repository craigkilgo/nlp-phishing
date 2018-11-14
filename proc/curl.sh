#!/bin/bash
curl -s -L $@ | grep -Eoi 'href=['\''"][0-9A-Za-z/_\.\,\-]*['\''"\?]' | sed -e "s#['\"\?]##g" | cut -f 2 -d '='
