PDF_OUT = build/MSc_group_k.pdf
SOURCES = metadata.yaml $(sort $(wildcard sections/*.md))

all:
	pandoc $(SOURCES) -o $(PDF_OUT) --extract-media=

clean:
	rm $(PDF_OUT)