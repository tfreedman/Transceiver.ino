Mailbox
=======

Mailbox is a collection of scripts to send and receive data between an Arduino and an XMPP client, over a 433MHz RF link and a second Arduino. It looks something like this:

=======                     =======             ==========================
Arduino <-- 433MHz Link --> Arduino <-- USB --> Computer running XMPPduino <-- Google Talk / XMPP Server --> Pidgin
=======                     =======             ==========================

It seems kind of pointless to send data over so many mediums, compared to just equipping a single Arduino with a WiFi card. However, there are numerous advantages to using a second Arduino to relay data to / from a network.

Arduino network cards are very expensive. You could have a device that monitors a sensor and outputs the data over WiFi, but an Arduino network card is around $60. If you wanted to monitor multiple sensors within a small area, without running wires between each sensor, you'd be paying $60+ per sensor, just in networking equipment. With multiple sensors, this would get very expensive, very fast.

433MHz transceivers, (such as this one: https://secure.robotshop.com/productinfo.aspx?pc=RB-Ons-09&lang=en-US) cost around $4, and can be used to send / receive small amounts of data. With the release of Digispark and other low cost Arduino clones, you can feasibly have a complete sensor package for around $15.

Why XMPP? XMPP is ubiquitous, and there are clients that run on nearly every device. Facebook Chat, Google Talk, and Microsoft Messenger are XMPP based (Messenger isn't, but has an XMPP API). You could therefore create a Facebook friend that you message on your phone, which acts as a serial console for the Arduino, or receive messages in Messages for OS X when a sensor is tripped. XMPP is excellent for this purpose, as it's instant, unlike email, and SMS potentially has fees.

Transceiver.ino contains code for receiving packets over the air and dumping them over serial, as well as receiving packets over serial and sending them out over the air. Messages prefixed with a ':' won't be printed to the serial console, and will be sanitized to remove the first ':' before being sent over the network (to prevent the same message from coming back to the sender program. XMPPduino prefixes all outgoing serial packets with a ':' automatically, though the serial console or a tty will not.

Mailbox.ino contains an example of a sensor on a remote Arduino. The sensor is monitoring a mailbox for snail mail deliveries, and will send a message to the USB-connected Arduino if a button is depressed. The USB-connected Arduino will then relay over Google Talk "You've Got Mail!" to a particular contact, but plenty of other sensors can be developed.

XMPPduino is a C# client program (Windows based, should work with Mono though) for sending and receiving messages over XMPP. Serial data is sent out to XMPP friends from the connected transceiver, and received XMPP messages are then broadcast by the transceiver back out over the 433MHz network.

TODO:

- Publish Config.cs (configuration file) and switch to XML config
- Add RDT wrapper over VirtualWire library
- Reduce power usage of Mailbox.ino
- Reduce XMPP spamming from Arduinos
- Publish schematics for Transceiver / Mailbox devices

LICENSE:

This code makes heavy use of VirtualWire and agsXMPP, both of which are GPLv2 licensed. Therefore, this code is also licensed under GPLv2.
