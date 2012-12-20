#include <VirtualWire.h>

char inData[20];
char inChar;
byte index = 0;

void setup(void) {
  Serial.begin(9600); //Set Baud Rate
  Serial.println("Init");
  vw_set_tx_pin(8);
  vw_set_rx_pin(7);
  vw_set_ptt_inverted(false); // Required for DR3100
  vw_setup(1200);	 // Bits per sec
  vw_rx_start();   // Start the receiver PLL running
}

void loop(void) {
  uint8_t buf[VW_MAX_MESSAGE_LEN];
  uint8_t buflen = VW_MAX_MESSAGE_LEN;
  
  if (Serial.available() > 0) {
    while(Serial.available() > 0) {
      inChar = Serial.read();
      if (inChar == '\r' || inChar == '\n')
        inChar == '\0';
      inData[index] = inChar;
      index++;
      inData[index] = '\0';
    }
    if (inData[0] != ':')
      Serial.println(inData);
    else {
      int i = 1;
      while (inData[i] != '\0') {
        inData[i-1] = inData[i];
        i++;
      }
      inData[i-1] = '\0';
    }
    Serial.flush();
    index = 0;
  }

  if (inData[0] != '\0') {
    vw_send((uint8_t *)inData, strlen(inData));
    vw_wait_tx();
    for (int i = 0; i < 20; i++)
      inData[i] = '\0';
  }

  if (vw_get_message(buf, &buflen)) {
    for (int i = 0; i < buflen; i++) {
      Serial.print((char)buf[i]);
    }
    Serial.println();
    Serial.flush();
  }
}

