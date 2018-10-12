#include <iostream>
#include <climits>
#include <vector>
#include <bitset>
#include <random>

/* This program is entirely hard coded.
 *The only variables you need to change are the macro constants "LENGTH",
 *which is how many bytes (generated) should be compressed,
 *and "PROPORTION", the percentage of the data that should be 0s (as a double).
 *NOTE: At the moment this program only works if the data given is predominantly 0s
*/
int main() {

  #define LENGTH 200 // EDITABLE
  #define PROPORTION 0.97 // EDITABLE

  // If there are more than 20 bytes, don't bother displaying them as it will be too large to read (if LARGE is defined later it will not print them)
  #if LENGTH > 20
  #define LARGE
  #endif

  // Create a byte array of 0s that is LENGTH long
  uint8_t* bytes = new uint8_t[LENGTH] {0};

  long length = LENGTH;

  // Iterate through each byte...
  for(auto i = 0; i < LENGTH; i++)
  {
    // ... and then iterate through each bit of the byte (CHAR_BIT is length of byte)
    for(auto j = 0; j < CHAR_BIT; j++)
    {
      // Generate a random double between 0 and 1
      std::random_device rd;
      std::mt19937 gen(rd());
      std::uniform_real_distribution<> dis(0, 1);

      double r = dis(gen);
      
      // Approx. 1 - PROPORTION % of the time the digit will be a 1
      if (r > PROPORTION)
      {
        bytes[i] |= 1UL << j;
      }
      // Else it is a 0
      else
      {
        bytes[i] &= ~(1UL << j);
      }
    }
  }

  // Printing metadata
  std::cout << "Uncompressed data: (Size: " << sizeof(*bytes) * LENGTH << ")" << std::endl;

  // If less than 20 bytes, print the bytes
  #ifndef LARGE
  for(auto i = 0; i < length; i++)
  {
    // Unsigned() is required for the std::cout to properly interprete uint8_t as a number
    std::cout << unsigned(bytes[i]) << std::endl;
  }
  #endif

  // Printing metadata
  std::cout << "Compressing..." << std::endl;

  // Create a vector of bytes
  std::vector<uint8_t>* a = new std::vector<uint8_t>;

  // Iterate through each byte...
  for(int i = 0; i < LENGTH; i++)
  {
    // ... and then iterate through each bit of the byte (CHAR_BIT is length of byte)
    for(auto j = 0; j < CHAR_BIT; j++)
    {
      /* Checks the value of the bit using masks. If it is 1, adds the index of it (i is *current byte, j is current bit, so i * 8 + j is the current bit overall) to the *vector
      */
      if (bytes[i] & (1 << j))
      {
        a->push_back(i * 8 + j);
      }
    }
  }

  // Delete the original array as it is no longer necessary
  delete[] bytes;

  // Printing metadata
  std::cout << "Finished compressing" << std::endl;
  std::cout << "Compressed data: (Size: " << a->size() << ")" << std::endl;

  // If less than 20 bytes, print the bytes
  #ifndef LARGE
  for(std::vector<uint8_t>::const_iterator i = a->begin(); i != a->end(); i++)
  {
    // Unsigned() is required for the std::cout to properly interprete uint8_t as a number
    std::cout << unsigned(*i) << std::endl;
  }
  #endif

  // Printing metadata
  std::cout << "Decompressing..." << std::endl;

  // Create an array to dump the decompressed data
  uint8_t decomp[LENGTH] = {0};

  // Iterate through each item (index) in the vector a
  for(std::vector<uint8_t>::const_iterator i = a->begin(); i != a->end(); i++)
  {
    // Set bit of the decompressed data based off the index of each '1' bit
    decomp[*i / 8] |= 1UL << (*i % 8);
  }

  // Delete the vector
  delete a;

  // Printing metadata
  std::cout << "Decompressed data: (Size: " << sizeof(decomp) << ")" << std::endl;
  
  #ifndef LARGE
  // Iterate through decompressed data
  for(auto i = 0; i < length; i++)
  {
    // Unsigned() is required for the std::cout to properly interprete uint8_t as a number
    std::cout << unsigned(decomp[i]) << std::endl;
  }
  #endif
}